using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Torch;

namespace RPGPlugin
{
    public class MinerConfig : ViewModel
    {
        // Definition of the ExpRatio property, which stores experience point values for individual minerals
        private Dictionary<string, double> _expRatio;
        private static readonly object _lock = new object();
        private static TimeSpan _lockTimeOut = TimeSpan.FromMilliseconds(5000);
        private const string storagePath = "Instance/RPGPlugin/";
        private static string configFilePath = Path.Combine(storagePath, "MinerConfig.json");
        
        private static MinerConfig defaultConfig = new MinerConfig
        {
            ExpRatio = new Dictionary<string, double>
            {
                ["Stone"] = 0.0013,
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
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_lock, _lockTimeOut, ref lockTaken);
                if (lockTaken)
                {
                    if (!Directory.Exists(storagePath))
                        Directory.CreateDirectory(storagePath);

                    if (!File.Exists(configFilePath))
                    {
                        // Return the default, no point saving a copy of defaultConfig.  Plus in the lock it cant be saved.
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
                            File.Move(configFilePath,
                                Path.Combine(storagePath, "MinerConfig_ERROR.json")); // Renames the file.
                            Roles.Log.Error($"There was an issue loading the MinerConfig.json configuration file.  The file has renamed too MinerConfig_ERROR.json and a clean default Miner configuration file created.");
                            Roles.Log.Error(e);
                            SaveMinerConfig(defaultConfig);

                            return defaultConfig;
                        }
                    }
                }
                else
                {
                    // Unable to get lock and load data!
                    Roles.Log.Error(
                        $"Unable to load MinerConfig.json, {configFilePath} is locked by another process. Using default values.");
                    return defaultConfig;
                }
            }
            catch (Exception e)
            {
                Roles.Log.Error(e);
                return defaultConfig;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_lock);
            }
            
        }

        // Method used to save the configuration file
        public static Task<bool> SaveMinerConfig(MinerConfig config)
        {
            // Task instead of void, run IO process in await.
            // return bool.  Doesnt need to be used but can be if needed.
            bool lockTaken = false;
            Monitor.TryEnter(_lock, _lockTimeOut, ref lockTaken);
            try
            {
                if (!Directory.Exists(storagePath))
                    Directory.CreateDirectory(storagePath);
                
                try
                {
                    // Serialize the configuration to JSON and save it
                    string jsonData = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText(configFilePath, jsonData);
                    return Task.FromResult(true);
                }
                catch (Exception e)
                {
                    // In case of an error while saving the file, log it
                    Roles.Log.Warn(e);
                    return Task.FromResult(false);
                }
            }
            catch (Exception e)
            {
                Roles.Log.Error(e);
                return Task.FromResult(false);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_lock);
            }
        }
    }
}







