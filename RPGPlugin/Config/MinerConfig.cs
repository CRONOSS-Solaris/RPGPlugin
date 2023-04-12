using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Torch;

namespace RPGPlugin
{
    public class MinerConfig : ViewModel
    {
        // Definition of the ExpRatio property, which stores experience point values for individual minerals
        private Dictionary<string, double> _expRatio;

        public Dictionary<string, double> ExpRatio
        {
            get => _expRatio;
            set => SetValue(ref _expRatio, value);
        }

        // Method used to load the configuration file
        public static MinerConfig LoadMinerConfig(string storagePath)
        {
            string configFilePath = Path.Combine(storagePath, "RPGPlugin", "MinerConfig.json");

            if (!File.Exists(configFilePath))
            {
                MinerConfig defaultConfig = new MinerConfig
                {
                    ExpRatio = new Dictionary<string, double>
                    {
                        ["Stone"] = 1,
                        ["Silicon"] = 1.2,
                        ["Iron"] = 1.3,
                        ["Nickel"] = 1.3,
                        ["Cobalt"] = 1.8,
                        ["Magnesium"] = 2.4,
                        ["Silver"] = 1.5,
                        ["Gold"] = 2.5,
                        ["Platinum"] = 2.8,
                        ["Uranium"] = 3.0,
                        ["Ice"] = 1.35
                    }
                };

                // Save the default configuration and return it
                SaveMinerConfig(defaultConfig, storagePath);
                return defaultConfig;
            }
            else
            {
                // If the file exists, load it
                try
                {
                    string jsonData = File.ReadAllText(configFilePath);
                    return JsonConvert.DeserializeObject<MinerConfig>(jsonData);
                }
                catch (Exception e)
                {
                    // In case of an error while loading the file, log it and return a null value
                    Roles.Log.Warn(e);
                    return null;
                }
            }
        }

        // Method used to save the configuration file
        public static void SaveMinerConfig(MinerConfig config, string storagePath)
        {
            string configFilePath = Path.Combine(storagePath, "RPGPlugin", "MinerConfig.json");

            try
            {
                // Serialize the configuration to JSON and save it
                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configFilePath, jsonData);
            }
            catch (Exception e)
            {
                // In case of an error while saving the file, log it
                Roles.Log.Warn(e);
            }
        }
    }
}







