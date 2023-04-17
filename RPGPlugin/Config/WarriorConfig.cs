//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;
//using Newtonsoft.Json;

//namespace RPGPlugin
//{
//    public sealed class WarriorConfig
//    {
//        // Definition of the ExpRatio property, which stores experience point values for individual minerals
//        private static readonly object _lock = new object();
//        private static TimeSpan _lockTimeOut = TimeSpan.FromMilliseconds(5000);
//        private const string storagePath = "Instance/RPGPlugin/";
//        private static string configFilePath = Path.Combine(storagePath, "WarriorConfig.json");
//        public ObservableCollection<KeyValuePair<string, double>> ExpRatio = new ObservableCollection<KeyValuePair<string, double>>();

//        private static WarriorConfig defaultConfig = new WarriorConfig
//        {
//            ExpRatio = new ObservableCollection<KeyValuePair<string, double>>
//            {
//                new KeyValuePair<string, double>(  "EnemyPlayer",         100  ),
//                new KeyValuePair<string, double>(  "FriendlyPlayer",      0    ),
//                new KeyValuePair<string, double>(  "EnemySmallBlock",     2    ),
//                new KeyValuePair<string, double>(  "EnemyLargeBlock",     10   ),
//                new KeyValuePair<string, double>(  "FriendlySmallBlock",  0    ),
//                new KeyValuePair<string, double>(  "FriendlyLargeBlock",  0    )
//            }
//        };

//        // Method used to load the configuration file
//        public async Task LoadWarriorConfig()
//        {
//            bool lockTaken = false;
//            try
//            {
//                Monitor.TryEnter(_lock, _lockTimeOut, ref lockTaken);
//                if (lockTaken)
//                {
//                    if (!Directory.Exists(storagePath))
//                        Directory.CreateDirectory(storagePath);

//                    if (!File.Exists(configFilePath))
//                    {
//                        // Apply the default, no point saving a copy of defaultConfig.  Plus in the lock it cant be saved.
//                        await LoadSettings();
//                    }
//                    else
//                    {
//                        // If the file exists, load it
//                        try
//                        {
//                            string jsonData = File.ReadAllText(configFilePath);
//                            WarriorConfig newConfig = JsonConvert.DeserializeObject<WarriorConfig>(jsonData);

//                            // Process newly loaded settings
//                            await LoadSettings(newConfig);

//                        }
//                        catch (Exception e)
//                        {
//                            File.Move(configFilePath, Path.Combine(storagePath, "WarriorConfig_ERROR.json")); // Renames the file.
//                            Roles.Log.Error($"There was an issue loading the WarriorConfig.json configuration file.  The file has renamed too WarriorConfig_ERROR.json and a clean default Warrior configuration file created.");
//                            Roles.Log.Error(e);
//                            await SaveWarriorConfig();
//                            await LoadSettings();
//                        }
//                    }
//                }
//                else
//                {
//                    // Unable to get lock and load data!
//                    Roles.Log.Error(
//                        $"Unable to load WarriorConfig.json, {configFilePath} is locked by another process. Using default values.");
//                    await LoadSettings();
//                }
//            }
//            catch (Exception e)
//            {
//                Roles.Log.Error(e);
//                await LoadSettings();
//            }
//            finally
//            {
//                if (lockTaken)
//                    Monitor.Exit(_lock);
//            }

//        }

//        private Task LoadSettings(WarriorConfig config = null)
//        {
//            ExpRatio = config == null ? defaultConfig.ExpRatio : config.ExpRatio;
//            return Task.CompletedTask;
//        }

//        // Method used to save the configuration file
//        public Task<bool> SaveWarriorConfig()
//        {
//            // Task instead of void, run IO process in await.
//            // return bool.  Doesnt need to be used but can be if needed.
//            bool lockTaken = false;
//            Monitor.TryEnter(_lock, _lockTimeOut, ref lockTaken);
//            try
//            {
//                if (!Directory.Exists(storagePath))
//                    Directory.CreateDirectory(storagePath);

//                try
//                {
//                    // Serialize the configuration to JSON and save it
//                    string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
//                    File.WriteAllText(configFilePath, jsonData);
//                    return Task.FromResult(true);
//                }
//                catch (Exception e)
//                {
//                    // In case of an error while saving the file, log it
//                    Roles.Log.Warn(e);
//                    return Task.FromResult(false);
//                }
//            }
//            catch (Exception e)
//            {
//                Roles.Log.Error(e);
//                return Task.FromResult(false);
//            }
//            finally
//            {
//                if (lockTaken)
//                    Monitor.Exit(_lock);
//            }
//        }
//    }
//}