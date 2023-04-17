using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows.Controls;
using System.Xml.Serialization;
using RPGPlugin.PointManagementSystem;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Managers.PatchManager;
using Torch.Session;
using VRage.GameServices;

namespace RPGPlugin
{
    public class Roles : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static ConcurrentDictionary<long, PlayerManager> PlayerManagers = new ConcurrentDictionary<long, PlayerManager>();
        private Timer _delayManagers = new Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
        public PatchManager patchManager;
        public PatchContext patchContext;
        public PointManager PointsManager = new PointManager();
        public static IChatManagerServer ChatManager => Instance.Torch.CurrentSession.Managers.GetManager<IChatManagerServer>();

        public static Roles Instance { get; private set; }
        public MinerConfig minerConfig = new MinerConfig();
        public HunterConfig hunterConfig = new HunterConfig();
        //public WarriorConfig warriorConfig = new WarriorConfig();
        public bool ServerOnline;
        public bool DelayFinished;

        public RolesControl _control;
        
        public UserControl GetControl() => _control ?? (_control = new RolesControl(this));
        private Persistent<RPGPluginConfig> _config;
        public RPGPluginConfig Config => _config?.Data;

        public override async void Init(ITorchBase torch)
        {
            base.Init(torch);
            Instance = this;
            SetupConfig();
            _delayManagers.Stop();
            _delayManagers.Elapsed += DelayManagersOnElapsed;
            TorchSessionManager sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");

            patchManager = DependencyProviderExtensions.GetManager<PatchManager>(torch.Managers);
            patchContext = patchManager.AcquireContext();
            Patches.DrillPatch.Patch(patchContext);
            await minerConfig.LoadMinerConfig();
            //await warriorConfig.LoadWarriorConfig();
            //await hunterConfig.LoadHunterConfig();
            Save();
        }

        private void DelayManagersOnElapsed(object sender, ElapsedEventArgs e)
        {
            Log.Warn("Delay Timer has finished.");
            PointsManager.Start();
            if (!ServerOnline) return;
            if (MySession.Static.SessionSimSpeedServer < 0.7) return;
            _delayManagers.Stop();
            _delayManagers.Elapsed -= DelayManagersOnElapsed;

            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
            {
                PlayerManager _pm = new PlayerManager();
                _pm.InitAsync(player.Id.SteamId);
                Log.Warn("Loaded data for player " + player.DisplayName);
                if (!PlayerManagers.TryAdd(player.Identity.IdentityId, _pm))
                {
                    Log.Error($"Player {player.DisplayName} [{player.Id.SteamId}] datafile could not be loaded.");
                }
            }

            MyMultiplayer.Static.ClientJoined += PlayerConnected;
            DelayFinished = true;
           
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {
            switch (state)
            {
                case TorchSessionState.Loaded:
                    Log.Info("Session Loaded!");
                    _delayManagers.Start();
                    ServerOnline = true;
                    break;

                case TorchSessionState.Unloading:
                    ServerOnline = false;
                    Log.Info("Session Unloading!");
                    MyMultiplayer.Static.ClientJoined -= PlayerConnected;
                    MyMultiplayer.Static.ClientLeft -= PlayerDisconnected;
                    SaveAllPlayersForShutDown();
                    break;
            }
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
                Log.Info("Roles Tracking Finished For " + manager.Value.GetSteamID());
            }
            PlayerManagers.Clear();
        }

        private void PlayerConnected(ulong steamID, string s)
        {
            MyPlayer player = MySession.Static.Players.TryGetPlayerBySteamId(steamID);
            PlayerManager roleManager = new PlayerManager();
            roleManager.InitAsync(steamID);
            PlayerManagers.TryAdd(player.Identity.IdentityId, roleManager);
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
                Log.Info("Main Configuration Saved.");
            }
            catch (IOException e)
            {
                Log.Warn(e, "Main Configuration Saved during plugin loading.");
            }
        }
    }
}
