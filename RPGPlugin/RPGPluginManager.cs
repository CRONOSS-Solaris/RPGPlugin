using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Sandbox.Game;
using Sandbox.Game.World;
using VRageMath;

namespace RPGPlugin
{
    public class PlayerManager
    {
        private const string DATA_DIRECTORY = "Instance/RPGPlugin/Player Data/";
        private const string DATA_FILE_EXTENSION = ".xml";
        public PlayerData _PlayerData = new PlayerData();
        
        public enum FromRoles {NoRole, Miner, Warrior}

        public PlayerManager() { }

        public async void InitAsync(ulong steamId)
        {
            await LoadPlayerData(steamId);
        }

        public async void SetRole(FromRoles role)
        {
            _PlayerData.SelectedRole = role;
            await SavePlayerData();
        }

        public FromRoles GetRole()
        {
            return _PlayerData.SelectedRole;
        }

        private Task LoadPlayerData(ulong steamId)
        {
            lock (Roles._FILELOCK)
            {
                string fileName = steamId + DATA_FILE_EXTENSION;
                string filePath = Path.Combine(DATA_DIRECTORY, fileName);

                if (!File.Exists(filePath))
                {
                    if (!MySession.Static.Players.TryGetPlayerBySteamId(steamId, out MyPlayer player))
                    {
                        Roles.Log.Error("Unable to get player by SteamID, new player file creation failed.");
                        return Task.CompletedTask;
                    }
                    _PlayerData.CreateNew(steamId, player.Identity.IdentityId);
                    return Task.CompletedTask;
                }

                try
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        var serializer = new XmlSerializer(typeof(PlayerData));
                        var playerData = (PlayerData)serializer.Deserialize(reader);
                        _PlayerData = playerData;
                        return Task.CompletedTask;
                    }
                }
                catch (Exception e)
                {
                    Roles.Log.Warn(e);
                    return Task.FromResult(new PlayerData());
                }
            }
        }

        public Task SavePlayerData()
        {
            lock(Roles._FILELOCK)
            {
                if (_PlayerData == null)
                {
                    // This should never be null.  If it is, something went wacko.  Good to have it though!
                    Roles.Log.Error("Null PlayerData on Save!!!");
                    return Task.CompletedTask;
                }
            
                string fileName = _PlayerData.SteamId + DATA_FILE_EXTENSION;
                string filePath = Path.Combine(DATA_DIRECTORY, fileName);

                try
                {
                    using (var writer = new StreamWriter(filePath))
                    {
                        Roles.Log.Fatal(filePath); // FOR DEBUGGING
                        var serializer = new XmlSerializer(typeof(PlayerData));
                        serializer.Serialize(writer, _PlayerData);
                    }
                }
                catch (Exception e)
                {
                    Roles.Log.Warn(e);
                }
            
                return Task.CompletedTask;
            }
        }
        
        public async Task AddMinerExp(double exp)
        {
            /* level  2    requires  67 points
               level  20   requires  668 points
               level  50   requires  1,670 points
               level  100  requires  3,340 points
               never ending levels!  Players need bragging rights, even miners!
             */
            double expForLevelUp = (_PlayerData.MinerLevel * (32.4 + _PlayerData.MinerLevel));

            if (_PlayerData.MinerExp + exp >= expForLevelUp)
            {
                _PlayerData.MinerLevel++;
                _PlayerData.MinerExp = Math.Round(_PlayerData.MinerExp + exp) - expForLevelUp;
                await SavePlayerData();
                MyVisualScriptLogicProvider.SendChatMessageColored("You have leveled up!!!", Color.Green, "Roles", _PlayerData.PlayerID);
            }
            else
            {
                _PlayerData.MinerExp += exp;
                Roles.Log.Warn($"Your EXP = {_PlayerData.MinerExp.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        public int ExpToLevelUp()
        {
            int expForLevelUp = (int)Math.Round(_PlayerData.MinerLevel * (32.4 + _PlayerData.MinerLevel));
            return (int)Math.Round(expForLevelUp - _PlayerData.MinerExp);
        }
    }
}
