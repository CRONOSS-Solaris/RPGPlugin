using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RPGPlugin
{
    /// <summary>
    /// Base class for all class configurations.
    /// </summary>
    /// <ToLoad name="GetConfig()">GetConfig().Result returns config data in json format.  Null if no config file or there was an error loading it.</ToLoad>
    /// <ToSave name="SaveConfig(string data)">SaveConfig(string data) saves your data to its own configuration file.  Data must be in json format.</ToSave>
    public abstract class configBase
    {
        private static readonly object _lock = new object();
        private static TimeSpan _lockTimeOut = TimeSpan.FromMilliseconds(5000);
        private const string storagePath = "Instance/RPGPlugin/";
        
        /// <summary>
        /// Store your class point type and point value per type in this collection.
        /// </summary>
        public abstract ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; }
        
        //test skill point system
        public abstract ObservableCollection<KeyValuePair<int, int>> SkillPoints { get; set; }
        
        /// <summary>
        /// Initializer, always start with base.int(). This sets the config file save location and name.
        /// </summary>
        public virtual Task init()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Register the class type in the master list!
        /// </summary>
        public abstract Task RegisterClass();

        protected virtual string GetFileName()
        {
            string type = GetType().Name;
            return Path.Combine(storagePath, GetType().Name + ".cfg");
        }

        /// <summary>
        /// GetConfig().Results returns your your config data as json. 
        /// </summary>
        public abstract Task LoadConfig();
        
        /// <summary>
        /// The following SHOULD work in most cases: 
        /// string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
        /// await SaveConfig(jsonData);
        /// </summary>
        public abstract Task SaveConfig();
        
        /// <summary>
        /// Call LoadConfig to load your config settings in Json format.
        /// Returns NULL if the file doesnt exist or there was a problem.
        /// </summary>
        protected virtual async Task<string> GetConfig()
        {
            return await Task.FromResult(GetConfigAsync().Result);
        }

        private Task<string> GetConfigAsync()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_lock, _lockTimeOut, ref lockTaken);
                if (lockTaken)
                {
                    if (!Directory.Exists(storagePath))
                        Directory.CreateDirectory(storagePath);

                    if (!File.Exists(GetFileName()))
                    {
                        // Apply the default, no point saving a copy of defaultConfig.  Plus in the lock it cant be saved.
                        // Nothing to do, will save first version of config when the save their first settings.
                        return Task.FromResult<string>(null);
                    }
                    else
                    {
                        // If the file exists, load it
                        try
                        {
                            string jsonData = File.ReadAllText(GetFileName());
                            return Task.FromResult(jsonData);

                        }
                        catch (Exception e)
                        {
                            File.Move(GetFileName(), Path.Combine(storagePath, "MinerConfig_ERROR.json")); // Renames the file.
                            Roles.Log.Error($"There was an issue loading the MinerConfig.json configuration file.  The file has renamed too MinerConfig_ERROR.json and a clean default Miner configuration file created.");
                            Roles.Log.Error(e);
                            return Task.FromResult<string>(null);
                        }
                    }
                }
                else
                {
                    // Unable to get lock and load data!
                    Roles.Log.Error($"Unable to load MinerConfig.json, {GetFileName()} is locked by another process. Using default values.");
                    return Task.FromResult<string>(null);
                }
            }
            catch (Exception e)
            {
                Roles.Log.Error(e);
                return Task.FromResult<string>(null);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_lock);
            }
        }
        
        /// <summary>
        /// Saves your config, returns true when successful or false when their was an error.
        /// </summary>
        /// <param name="data">Your config file in json format.</param>
        protected Task<bool> SaveConfig(string data)
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
                    File.WriteAllText(GetFileName(), data);
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