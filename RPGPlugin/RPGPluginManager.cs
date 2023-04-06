using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace RPGPlugin
{
    public class RoleManager
    {
        private const string DATA_DIRECTORY = "RPGPlugin/Player Data/";
        private const string DATA_FILE_EXTENSION = ".xml";

        private readonly Dictionary<string, IRole> _roles = new Dictionary<string, IRole>();
         
        public RoleManager()
        {
            // dodajemy dostępne role
            AddRole(new Miner());
            // dodaj kolejne role tutaj, np. AddRole(new Warrior());
        }

        public void AddRole(IRole role)
        {
            _roles.Add(role.GetType().Name, role);
        }

        public bool SetRole(ulong steamId, string roleName)
        {
            if (!_roles.ContainsKey(roleName))
            {
                return false; // jeśli rola nie istnieje, nie ustawiamy jej dla gracza
            }

            var playerData = LoadPlayerData(steamId);
            if (playerData != null)
            {
                playerData.SelectedRole = roleName;
                SavePlayerData(playerData);
                return true;
            }
            return false;
        }

        public IRole GetRole(ulong steamId)
        {
            var playerData = LoadPlayerData(steamId);
            if (playerData == null || !_roles.ContainsKey(playerData.SelectedRole))
            {
                return null;
            }

            return _roles[playerData.SelectedRole];
        }

        private PlayerData LoadPlayerData(ulong steamId)
        {
            string fileName = steamId + DATA_FILE_EXTENSION;
            string filePath = Path.Combine(DATA_DIRECTORY, fileName);

            if (!File.Exists(filePath))
            {
                return null;
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

        private void SavePlayerData(PlayerData playerData)
        {
            string fileName = playerData.SteamId + DATA_FILE_EXTENSION;
            string filePath = Path.Combine(DATA_DIRECTORY, fileName);

            try
            {
                Directory.CreateDirectory(DATA_DIRECTORY);
                using (var writer = new StreamWriter(filePath))
                {
                    var serializer = new XmlSerializer(typeof(PlayerData));
                    serializer.Serialize(writer, playerData);
                }
            }
            catch (Exception e)
            {
                Roles.Log.Warn(e);
            }
        }
    }
}
