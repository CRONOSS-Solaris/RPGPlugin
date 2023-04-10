using System;
using System.IO;
using System.Xml.Serialization;
using Sandbox.Game.World;

namespace RPGPlugin
{
    public class RoleManager
    {
        private const string DATA_DIRECTORY = "RPGPlugin/Player Data/";
        private const string DATA_FILE_EXTENSION = ".xml";
        public PlayerData PlayerData = null;

        public enum FromRoles {NoRole, Miner, Warrior}

        public RoleManager() { }

        public bool SetRole(ulong steamId, FromRoles role)
        {
            PlayerData playerData = LoadPlayerData(steamId);
            if (playerData == null) return false;
            
            playerData.SelectedRole = role;
            SavePlayerData();
            return true;
        }

        public FromRoles GetRole(ulong steamId)
        {
            return PlayerData.SelectedRole;
        }

        private PlayerData LoadPlayerData(ulong steamId)
        {
            string fileName = steamId + DATA_FILE_EXTENSION;
            string filePath = Path.Combine(DATA_DIRECTORY, fileName);

            if (!File.Exists(filePath))
            {
                // return null;  Instead of returning null, create a new dataset.  They are a new player maybe.
                PlayerData newPlayer = new PlayerData(steamId);
                MySession.Static.Players.TryGetPlayerBySteamId(steamId, out MyPlayer playerData);
                newPlayer.PlayerID = playerData.Identity.IdentityId;
                return newPlayer;
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
                Roles.Log.Warn(e);
                return null;
            }
        }

        public void SavePlayerData()
        {
            if (PlayerData == null)
            {
                // This should never be null.  If it is, something went wacko.  Good to have it though!
                return;
            }
            string fileName = PlayerData.SteamId + DATA_FILE_EXTENSION;
            string filePath = Path.Combine(DATA_DIRECTORY, fileName);

            try
            {
                Directory.CreateDirectory(DATA_DIRECTORY);
                using (var writer = new StreamWriter(filePath))
                {
                    var serializer = new XmlSerializer(typeof(PlayerData));
                    serializer.Serialize(writer, PlayerData);
                }
            }
            catch (Exception e)
            {
                Roles.Log.Warn(e);
            }
        }
    }
}
