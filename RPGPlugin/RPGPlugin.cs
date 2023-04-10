using NLog;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows.Controls;
using System.Xml.Serialization;
using Sandbox.Game;
using Sandbox.Game.World;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using VRage.Game.ModAPI;

namespace RPGPlugin
{
    public class Roles : TorchPluginBase, IWpfPlugin
    {
         
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static List<RoleManager> PlayerManagers = new List<RoleManager>();
        private static Timer AutoSaver = new Timer();

        // Instead of writing to their player data file every tick they mine, track and save
        // all the data periodically.  
        public static Dictionary<ulong, PlayerData> TrackOnlinePlayers = new Dictionary<ulong, PlayerData>();
        public static Dictionary<ulong, MyPlayer> OnlinePlayersList = new Dictionary<ulong, MyPlayer>();
        public static Roles Instance { get; private set; }
        
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
            AutoSaver.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;  

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");

            Save();
        }
        
        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {

            switch (state)
            {
                case TorchSessionState.Loaded:
                    Log.Info("Session Loaded!");
                    MyAPIGateway.Entities.OnEntityAdd += ExpMiningManager.OnEntityAdd;
                    
                    // When a player connects, this will load their data and update online player list.
                    MyVisualScriptLogicProvider.PlayerConnected += LoadPlayerData;
                    // When a player disconnects, this will save and unload their data and update online player list.
                    MyVisualScriptLogicProvider.PlayerDisconnected += PlayerDisconnected;
                    AutoSaver.Start();
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    MyAPIGateway.Entities.OnEntityAdd -= ExpMiningManager.OnEntityAdd;
                    MyVisualScriptLogicProvider.PlayerConnected -= LoadPlayerData;
                    MyVisualScriptLogicProvider.PlayerDisconnected -= PlayerDisconnected;
                    SaveAllLoadPlayerData();
                    AutoSaver.Stop();
                    break;
            }
        }

        private void PlayerDisconnected(long playerId)
        {
            // Save their data
            SavePlayerData(GetPlayerSteamID(playerId));
            
            // Unload them from the system, free up resources.
            for (int index = PlayerManagers.Count - 1; index >= 0; index--)
            {
                RoleManager manager = PlayerManagers[index];
                if (manager.PlayerData.PlayerID != playerId) continue;
                manager.SavePlayerData();
            }
        }

        private void SaveAllLoadPlayerData()
        {
            for (int index = PlayerManagers.Count - 1; index >= 0; index--)
            {
                PlayerManagers[index].SavePlayerData();
            }
        }

        public void SavePlayerData(ulong SteamID)
        {
            for (int index = PlayerManagers.Count - 1; index >= 0; index--)
            {
                RoleManager manager = PlayerManagers[index];
                if (manager.PlayerData.SteamId != SteamID) continue;
                manager.SavePlayerData();
            }
        }

        private void LoadPlayerData(long playerId)
        {
            RoleManager roleManager = new RoleManager();
            ulong SteamID = GetPlayerSteamID(playerId);
            roleManager.GetRole(SteamID);
            PlayerManagers.Add(roleManager);
            MyPlayer player = MySession.Static.Players.TryGetPlayerBySteamId(SteamID);
            OnlinePlayersList.Add(SteamID, player);
        }

        public ulong GetPlayerSteamID(long playerId)
        {
            var OnlinePlayers = new List<IMyPlayer>();
            MyAPIGateway.Multiplayer.Players.GetPlayers(OnlinePlayers);
            for (int index = OnlinePlayers.Count - 1; index >= 0; index--)
            {
                if (OnlinePlayers[index].Identity.IdentityId != playerId) continue;
                ulong SteamID = OnlinePlayers[index].SteamUserId;
                if (OnlinePlayersList.ContainsKey(SteamID))
                    OnlinePlayersList.Remove(SteamID);
                return OnlinePlayers[index].SteamUserId;
            }

            return 0; // Something went wrong 
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
