using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Controls;
using System.Xml.Serialization;
using RPGPlugin.PointManagementSystem;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Managers.PatchManager;
using Torch.Session;
using VRage.GameServices;
using Torch.Mod.Messages;
using Torch.Mod;
using VRage.Game.ModAPI;

namespace RPGPlugin
{
    public class Roles : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static ConcurrentDictionary<long, PlayerManager> PlayerManagers = new ConcurrentDictionary<long, PlayerManager>();
        private static Timer AutoSaver = new Timer();
        private static Timer DelayStart = new Timer();
        public bool DelayFinished;
        public PatchManager patchManager;
        public PatchContext patchContext;
        public static IChatManagerServer ChatManager => Instance.Torch.CurrentSession.Managers.GetManager<IChatManagerServer>();

        // Instead of writing to their player data file every tick they mine, track and save
        // all the data periodically.
        public static Dictionary<ulong, MyPlayer> OnlinePlayersList = new Dictionary<ulong, MyPlayer>();
        public static Roles Instance { get; private set; }
        public MinerConfig MinerConfig { get; private set; }

        private RolesControl _control;
        public UserControl GetControl() => _control ?? (_control = new RolesControl(this));
        private Persistent<RPGPluginConfig> _config;
        public RPGPluginConfig Config => _config?.Data;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            Instance = this;
            SetupConfig();
            
            // This is how often all online players data will be saved automatically
            AutoSaver.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;

            MinerConfig = MinerConfig.LoadMinerConfig();

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");

            patchManager = DependencyProviderExtensions.GetManager<PatchManager>(torch.Managers);
            patchContext = patchManager.AcquireContext();
            Patches.DrillPatch.Patch(patchContext);
            Save();
        }
        
        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {
            switch (state)
            {
                case TorchSessionState.Loaded:
                    Log.Info("Session Loaded!");
                    DelayStart.Interval = TimeSpan.FromSeconds(30).TotalMilliseconds;
                    DelayStart.Elapsed += DelayStartActivate;
                    DelayStart.Start();
                    Log.Info("Delay Start Timer Active!"); // DEBUG
                    
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    MyMultiplayer.Static.ClientJoined -= LoadPlayerData;
                    MyMultiplayer.Static.ClientLeft -= PlayerDisconnected;
                    SaveAllPlayersForShutDown();
                    DelayStart.Stop();
                    AutoSaver.Stop();
                    
                    break;
            }
        }

        private async void DelayStartActivate(object sender, ElapsedEventArgs e)
        {
            // This is because on server restart, players who rush to connect will trigger connect events
            // before the server is properly ready and will pass playerid of 0.  This is a standard problem
            // for both plugins and mods. Welcome to the wonderful world of Keen :=)
            Log.Info("Role DelayStart activated"); // DEBUG 
            DelayStart.Stop();
            DelayStart.Dispose();
             // DelayStart.Dispose();No longer need it, clean it up now.
             
            // When a player spawns their in-game data is loaded, now we load their data and update online player list.
            MyMultiplayer.Static.ClientJoined += LoadPlayerData;
            // When a player disconnects, this will save and unload their data and update online player list.
            MyMultiplayer.Static.ClientLeft += PlayerDisconnected;
            // Not the connect listeners are active, we can pick up the current logged in players and set them up.
            List<MyPlayer> onlinePlayers = Sync.Players.GetOnlinePlayers().ToList();
            
            foreach (MyPlayer player in onlinePlayers)
            {
                await PointManager.Start(); 
                LoadPlayerData(player.Id.SteamId, "something here? player name?");
                
                Log.Info("Role Data Active For " + player.Identity.DisplayName); // DEBUG
            }

            DelayFinished = true;
            onlinePlayers.Clear();
            AutoSaver.Start();
        }

        private async void PlayerDisconnected(ulong steamID, MyChatMemberStateChangeEnum myChatMemberStateChangeEnum)
        {
            // Unload them from the system, free up resources.
            MyPlayer player = MySession.Static.Players.TryGetPlayerBySteamId(steamID);
            
            if (!PlayerManagers.ContainsKey(player.Identity.IdentityId))
            {
                Log.Error($"Unable to save profile for player [IdentityID:{player.Identity.IdentityId}], it was probably not loaded.");
                return;
            }
            await PlayerManagers[player.Identity.IdentityId].SavePlayerData();
            PlayerManagers.Remove(player.Identity.IdentityId);
        }

        private async void SaveAllPlayersForShutDown()
        {
            foreach (KeyValuePair<long,PlayerManager> manager in PlayerManagers)
            {
                await manager.Value.SavePlayerData();
                Log.Info("Roles Tracking Finished For " + manager.Value._PlayerData.PlayerID);
            }
            PlayerManagers.Clear();
        }

        private void LoadPlayerData(ulong steamID, string s)
        {
            MyPlayer player = MySession.Static.Players.TryGetPlayerBySteamId(steamID);
            PlayerManager roleManager = new PlayerManager();
            roleManager.InitAsync(steamID);
            PlayerManagers.TryAdd(player.Identity.IdentityId, roleManager);
            if (!DelayFinished)
                OnlinePlayersList.Add(steamID, player);
        }

        // MOTD command style
        public void SendRoles(IMyPlayer player, string header, string message)
        {
            long playerId = player.Identity.IdentityId;
            ulong steamId = player.SteamUserId;

            var msg = new DialogMessage("RPGPLUGIN", header, message);
            ModCommunication.SendMessageTo(msg, steamId);
        }



        private void SetupConfig()
        {
            string dataPath = Path.Combine(StoragePath, "RPGPlugin");
            string playerDataPath = Path.Combine(dataPath, "Player Data");

            // create directories if they do not exist
            Directory.CreateDirectory(dataPath);
            Directory.CreateDirectory(playerDataPath);

            // set the config file path
            string configFile = Path.Combine(StoragePath, "RPGPluginConfig.xml");

            if (!File.Exists(configFile))
            {
                _config = new Persistent<RPGPluginConfig>(configFile);
                _config.Save();
            }
            else
            {
                try
                {
                    _config = Persistent<RPGPluginConfig>.Load(configFile);
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                }

                if (_config?.Data == null)
                {
                    Log.Info("Create Default Config, because none was found!");
                    _config = new Persistent<RPGPluginConfig>(configFile, new RPGPluginConfig());
                    _config.Save();
                }
                else
                {
                    try
                    {
                        var xmlSerializer = new XmlSerializer(typeof(RPGPluginConfig));
                        using (var streamReader = new StreamReader(configFile))
                        {
                            var configData = (RPGPluginConfig)xmlSerializer.Deserialize(streamReader);
                            _config = new Persistent<RPGPluginConfig>(configFile, configData);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e);
                    }
                }
            }
        }

        public void Save()
        {
            try
            {
                _config.Save();
                Log.Info("Configuration Saved.");
            }
            catch (IOException e)
            {
                Log.Warn(e, "Configuration failed to save");
            }
        }
    }
}
