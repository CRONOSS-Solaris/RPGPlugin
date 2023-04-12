using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Controls;
using System.Xml.Serialization;
using RPGPlugin.PointManagementSystem;
using RPGPlugin.Utils;
using Sandbox.Game;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;

namespace RPGPlugin
{
    public class Roles : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static Dictionary<long, PlayerManager> PlayerManagers = new Dictionary<long, PlayerManager>();
        private static Timer AutoSaver = new Timer();
        public PointManager ExpManager = new PointManager();
        private static Timer DelayStart = new Timer();
        public static object _FILELOCK = new object();
        private bool DelayFinished = false;

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
            ExpManager.Init(); // Load the config files

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
                    MyVisualScriptLogicProvider.PlayerConnected -= LoadPlayerData;
                    MyVisualScriptLogicProvider.PlayerDisconnected -= PlayerDisconnected;
                    MyVisualScriptLogicProvider.ShipDrillCollected -= ExpManager.ShipDrillCollected;
                    SaveAllPlayersForShutDown();
                    DelayStart.Stop();
                    AutoSaver.Stop();
                    break;
            }
        }

        private void DelayStartActivate(object sender, ElapsedEventArgs e)
        {
            // This is because on server restart, players who rush to connect will trigger connect events
            // before the server is properly ready and will pass playerid of 0.  This is a standard problem
            // for both plugins and mods. Welcome to the wonderful world of Keen :=)
            Log.Info("Role DelayStart activated"); // DEBUG 
            DelayStart.Stop();
            DelayStart.Dispose(); // No longer need it, clean it up now.
            // When a player spawns their ingame data is loaded, now we load their data and update online player list.
            MyVisualScriptLogicProvider.PlayerConnected += LoadPlayerData;
            // When a player disconnects, this will save and unload their data and update online player list.
            MyVisualScriptLogicProvider.PlayerDisconnected += PlayerDisconnected;
            MyVisualScriptLogicProvider.ShipDrillCollected += ExpManager.ShipDrillCollected;
            // Not the connect listeners are active, we can pick up the current logged in players and set them up.
            List<MyPlayer> onlinePlayers = Sync.Players.GetOnlinePlayers().ToList();
            foreach (MyPlayer player in onlinePlayers)
            {
                LoadPlayerData(player.Identity.IdentityId);
                Log.Info("Role Data Active For " + player.Identity.DisplayName); // DEBUG
            }

            DelayFinished = true;
            onlinePlayers.Clear();
            AutoSaver.Start();
        }

        private async void PlayerDisconnected(long playerId)
        {
            // Unload them from the system, free up resources.
            if (!PlayerManagers.ContainsKey(playerId))
            {
                Log.Error($"Unable to save profile for player [IdentityID:{playerId}], it was probably not loaded.");
                return;
            }
            await PlayerManagers[playerId].SavePlayerData();
            PlayerManagers.Remove(playerId);
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

        private void LoadPlayerData(long playerId)
        {
            ulong SteamID = GetPlayerSteamID(playerId);
            PlayerManager roleManager = new PlayerManager();
            roleManager.InitAsync(SteamID);
            PlayerManagers.Add(playerId, roleManager);
            MyPlayer player = MySession.Static.Players.TryGetPlayerBySteamId(SteamID);
            if (!DelayFinished)
                OnlinePlayersList.Add(SteamID, player);
        }

        private ulong GetPlayerSteamID(long playerId)
        {
            return MySession.Static.Players.TryGetSteamId(playerId);
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
