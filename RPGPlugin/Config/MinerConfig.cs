using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Torch;

namespace RPGPlugin
{
    // Needs file locking mechanism at some point.  If user has files open in another app to edit
    // and the files are locked by the app, this will fail and return null which should be a 
    // last resort.  
    
    public class MinerConfig : ViewModel
    {
        // Definition of the ExpRatio property, which stores experience point values for individual minerals
        private Dictionary<string, double> _expRatio;
        
        // This could be a configurable setting but for now this works.
        private const string storagePath = "Instance/RPGPlugin/";
        
        private static MinerConfig defaultConfig = new MinerConfig
        {
            ExpRatio = new Dictionary<string, double>
            {
                ["Stone"] = 0.13,
                ["Silicon"] = 0.12,
                ["Iron"] = 0.13,
                ["Nickel"] = 0.13,
                ["Cobalt"] = 0.18,
                ["Magnesium"] = 0.24,
                ["Silver"] = 0.15,
                ["Gold"] = 0.25,
                ["Platinum"] = 0.28,
                ["Uranium"] = 0.30,
                ["Ice"] = 0.135
            }
        };

        public Dictionary<string, double> ExpRatio
        {
            get => _expRatio;
            set => SetValue(ref _expRatio, value);
        }

        // Method used to load the configuration file
        public static MinerConfig LoadMinerConfig()
        {
            string configFilePath = Path.Combine(storagePath, "MinerConfig.json");

            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);

            if (!File.Exists(configFilePath))
            {
                // Save the default configuration and return it
                SaveMinerConfig(defaultConfig);
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
                    
                    // Instead of returning null (always avoid null data for players when possible)
                    // We can return a new configuration file.  If the user edits the file and breaks
                    // json formatting or the file is corrupt, we end up here.  We can setup specific
                    // exception catches for better handling also.
                    
                    File.Move(configFilePath,Path.Combine(storagePath, "MinerConfig_ERROR.json")); // Renames the file.
                    Roles.Log.Error($"There was an issue loading the MinerConfig.json configuration file.  The file has renamed too MinerConfig_ERROR.json and a clean default Miner configuration file created.");
                    SaveMinerConfig(defaultConfig);
                    Roles.Log.Warn(e);
                    
                    return defaultConfig;
                }
            }
        }

        // Method used to save the configuration file
        public static void SaveMinerConfig(MinerConfig config)
        {
            string configFilePath = Path.Combine(storagePath, "MinerConfig.json");
            
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);
            
            try
            {
                // Serialize the configuration to JSON and save it
                string jsonData = JsonConvert.SerializeObject(config, Formatting.Indented);
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







