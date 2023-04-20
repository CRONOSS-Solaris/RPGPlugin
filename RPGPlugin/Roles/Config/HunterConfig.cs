using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RPGPlugin
{
    public sealed class HunterConfig
    {
        // Definition of the ExpRatio property, which stores experience point values for individual minerals
        private static readonly object _lock = new object();
        private static TimeSpan _lockTimeOut = TimeSpan.FromMilliseconds(5000);
        private const string storagePath = "Instance/RPGPlugin/";
        private static string configFilePath = Path.Combine(storagePath, "HunterConfig.json");
        public ObservableCollection<KeyValuePair<string, double>> ExpRatio = new ObservableCollection<KeyValuePair<string, double>>();

        private static HunterConfig defaultConfig = new HunterConfig
        {
            ExpRatio = new ObservableCollection<KeyValuePair<string, double>>
            {
                new KeyValuePair<string, double>(  "Animals",     0.0013 ),
                new KeyValuePair<string, double>(  "SmallBlock",  2    ),
                new KeyValuePair<string, double>(  "LargeBlock",  10   )
            }
        };

        // Method used to load the configuration file
        public async Task LoadHunterConfig()
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
                        // Apply the default, no point saving a copy of defaultConfig.  Plus in the lock it cant be saved.
                        await LoadSettings(defaultConfig);
                    }
                    else
                    {
                        // If the file exists, load it
                        try
                        {
                            string jsonData = File.ReadAllText(configFilePath);
                            HunterConfig userConfig = JsonConvert.DeserializeObject<HunterConfig>(jsonData);

                            // Process newly loaded settings
                            await LoadSettings(userConfig);

                        }
                        catch (Exception e)
                        {
                            File.Move(configFilePath, Path.Combine(storagePath, "HunterConfig_ERROR.json")); // Renames the file.
                            Roles.Log.Error($"There was an issue loading the HunterConfig.json configuration file.  The file has renamed too HunterConfig_ERROR.json and a clean default Hunter configuration file created.");
                            Roles.Log.Error(e);
                            await LoadSettings(defaultConfig);
                        }
                    }
                }
                else
                {
                    // Unable to get lock and load data!
                    Roles.Log.Error(
                        $"Unable to load HunterConfig.json, {configFilePath} is locked by another process. Using default values.");
                    await LoadSettings(defaultConfig);
                }
            }
            catch (Exception e)
            {
                Roles.Log.Error(e);
                await LoadSettings(defaultConfig);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_lock);
            }

        }

        private Task LoadSettings(HunterConfig config)
        {
            ExpRatio = config.ExpRatio;
            return Task.CompletedTask;
        }

        // Method used to save the configuration file
        public Task<bool> SaveHunterConfig()
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
                    string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
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