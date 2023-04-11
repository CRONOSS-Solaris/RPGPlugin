using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Sandbox.Game.World;

namespace RPGPlugin.Utils
{
    public sealed class FileManager
    {
        // Here we request and save to player files.  This allows a clean lock and 
        // better controlled asynchronous access.
        private static object _fileLock = new object();
        private TimeSpan _lockTimeOut = TimeSpan.FromMilliseconds(500);
        private static bool lockTaken; 
        private const string DATA_DIRECTORY = "Instance/RPGPlugin/Player Data/";
        private const string DATA_FILE_EXTENSION = ".xml";
        
        
        public FileManager() { }
        
        private PlayerData _data(ulong steamID)
        {
            try
            {
                Monitor.TryEnter(_fileLock, _lockTimeOut, ref lockTaken);
                if (lockTaken)
                {
                    // Lock achieved, get data
                    string fileName = steamID + DATA_FILE_EXTENSION;
                    string filePath = Path.Combine(DATA_DIRECTORY, fileName);
                    
                    if (!File.Exists(filePath))
                    {
                        if (!MySession.Static.Players.TryGetPlayerBySteamId(steamID, out MyPlayer player))
                        {
                            // Add a check later to retry and get playerID at a later point and not fail here.
                            Roles.Log.Error("Unable to get player by SteamID, new player file creation failed.");
                        }

                        PlayerData newData = new PlayerData();
                        newData.CreateNew(steamID, player.Identity.IdentityId);
                        return newData;
                    }
                    
                    try
                    {
                        using (var reader = new StreamReader(filePath))
                        {
                            var serializer = new XmlSerializer(typeof(PlayerData));
                            var playerData = (PlayerData)serializer.Deserialize(reader);
                            return playerData;
                        }
                    }
                    catch (Exception e)
                    {
                        // This can be better managed to prevent player from losing progress but we
                        // must return something for the player to log their xp points into.
                        Roles.Log.Error("Unable to load player data, creating new file for player."); 
                        Roles.Log.Warn(e);

                        MySession.Static.Players.TryGetPlayerBySteamId(steamID, out MyPlayer player);
                        PlayerData tempData =  new PlayerData(); 
                        tempData.CreateNew(steamID, player.Identity.IdentityId);
                        return tempData;
                    }
                }
                else
                {
                    // Unable to get lock and load data!
                    return null;  // Requires adding null check in RPGPluginManager.cs
                }
            }
            catch (Exception e)
            {
                Roles.Log.Error(e);
                return null; // Requires adding null check in RPGPluginManager.cs
            }
        }

        private bool _data(PlayerData data)
        {
            try
            {
                Monitor.TryEnter(_fileLock, _lockTimeOut, ref lockTaken);
                if (lockTaken)
                {
                    // Lock achieved, save data
                    if (data == null)
                    {
                        // This should never be null.  If it is, something went wacko.  Good to have it though!
                        Roles.Log.Error("Null PlayerData on Save!!!");
                        return false;
                    }
            
                    string fileName = data.SteamId + DATA_FILE_EXTENSION;
                    string filePath = Path.Combine(DATA_DIRECTORY, fileName);

                    try
                    {
                        using (var writer = new StreamWriter(filePath))
                        {
                            Roles.Log.Fatal(filePath); // FOR DEBUGGING
                            var serializer = new XmlSerializer(typeof(PlayerData));
                            serializer.Serialize(writer, data);
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Roles.Log.Warn(e);
                    }

                    return false;
                }
                else
                {
                    // Unable to get lock and save data!
                    return false;  // Requires adding null check in RPGPluginManager.cs
                }
            }
            catch (Exception e)
            {
                Roles.Log.Error(e);
                return false; // Requires adding null check in RPGPluginManager.cs
            }
        }

        public Task<PlayerData> GetPlayerData(ulong steamID)  // Public async access to load data
        {
            return Task.FromResult(_data(steamID));
        }

        public Task<bool> SavePlayerData(PlayerData playerData) // public async access to save data
        {
            return Task.FromResult(_data(playerData));
        }
    }
}