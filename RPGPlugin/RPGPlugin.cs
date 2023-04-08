using NLog;
using Sandbox.ModAPI;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Xml.Serialization;
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

        public const ushort ExpMiningHandlerId = 54546; // Wybierz unikalne ID, które nie koliduje z innymi modyfikacjami

        private RolesControl _control;
        public UserControl GetControl() => _control ?? (_control = new RolesControl(this));

        private Persistent<RPGPluginConfig> _config;
        public RPGPluginConfig Config => _config?.Data;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            
            MyAPIGateway.Multiplayer.RegisterMessageHandler(ExpMiningHandlerId, ExpMiningHandler);


            SetupConfig();

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
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    break;
            }
        }

        private void ExpMiningHandler(byte[] data)
        {
            try
            {
                using (var stream = new MemoryStream(data))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        ulong steamId = reader.ReadUInt64();
                        string typeId = reader.ReadString();
                        string subtypeId = reader.ReadString();
                        int amount = reader.ReadInt32();

                        var roleManager = new RoleManager();
                        var role = roleManager.GetRole(steamId);
                        if (role != null)
                        {
                            role.AddExp(amount, typeId, subtypeId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
        }


        private void SetupConfig()
        {
            string dataPath = Path.Combine(StoragePath, "RPGPlugin");
            string playerdataPath = Path.Combine(dataPath, "Player Data");

            // create directories if they do not exist
            Directory.CreateDirectory(dataPath);
            Directory.CreateDirectory(playerdataPath);

            // set the config file path
            string configFile = Path.Combine(StoragePath, "RPGPluginConfig.xml");

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
