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
        public PlayerData PlayerData = new PlayerData();

        public enum FromRoles {NoRole, Miner, Warrior}

        public PlayerManager(ulong steamId)
        {
            
        }

        public async void InitAsync(ulong steamId)
        {
            await LoadPlayerData(steamId);
        }

        public async void SetRole(FromRoles role)
        {
            PlayerData.SelectedRole = role;
            await SavePlayerData();
        }

        public FromRoles GetRole()
        {
            return PlayerData.SelectedRole;
        }

        private Task LoadPlayerData(ulong steamId)
        {
            string fileName = steamId + DATA_FILE_EXTENSION;
            string filePath = Path.Combine(DATA_DIRECTORY, fileName);

            if (!File.Exists(filePath))
            {
                // return null;  Instead of returning null, create a new dataset.  They are a new player maybe.
                PlayerData newPlayer = new PlayerData();
                MySession.Static.Players.TryGetPlayerBySteamId(steamId, out MyPlayer player);
                PlayerData.CreateNew(steamId, player.Identity.IdentityId);
                return Task.CompletedTask;
            }

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    var serializer = new XmlSerializer(typeof(PlayerData));
                    var playerData = (PlayerData)serializer.Deserialize(reader);
                    PlayerData = playerData;
                    SavePlayerData();
                    return Task.CompletedTask;
                }
            }
            catch (Exception e)
            {
                Roles.Log.Warn(e);
                return Task.FromResult(new PlayerData());
            }
        }

        public Task SavePlayerData()
        {
            if (PlayerData == null)
            {
                // This should never be null.  If it is, something went wacko.  Good to have it though!
                Roles.Log.Error("Null PlayerData on Save!!!");
                return Task.CompletedTask;
            }
            
            string fileName = PlayerData.SteamId + DATA_FILE_EXTENSION;
            string filePath = Path.Combine(DATA_DIRECTORY, fileName);

            try
            {
                Directory.CreateDirectory(DATA_DIRECTORY);
                using (var writer = new StreamWriter(filePath))
                {
                    //DEBUG ONLY//
                    Roles.Log.Fatal(filePath); // FOR DEBUGGING
                    var serializer = new XmlSerializer(typeof(PlayerData));
                    serializer.Serialize(writer, PlayerData);
                }
            }
            catch (Exception e)
            {
                Roles.Log.Warn(e);
            }
            
            return Task.CompletedTask;
        }
        
        public async Task AddMinerExp(double exp)
        {
            /* level  2    requires  67 points
               level  20   requires  668 points
               level  50   requires  1,670 points
               level  100  requires  3,340 points
               never ending levels!  Players need bragging rights, even miners!
             */
            double expForLevelUp = (PlayerData.MinerLevel * (32.4 + PlayerData.MinerLevel));

            if (PlayerData.MinerExp + exp >= expForLevelUp)
            {
                PlayerData.MinerLevel++;
                PlayerData.MinerExp = Math.Round(PlayerData.MinerExp + exp) - expForLevelUp;
                await SavePlayerData();
                MyVisualScriptLogicProvider.SendChatMessageColored("You have leveled up!!!", Color.Green, "Roles", PlayerData.PlayerID);
            }
            else
            {
                PlayerData.MinerExp += exp;
                Roles.Log.Warn($"Your EXP = {PlayerData.MinerExp.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        public int ExpToLevelUp()
        {
            int expForLevelUp = (int)Math.Round(PlayerData.MinerLevel * (32.4 + PlayerData.MinerLevel));
            return (int)Math.Round(expForLevelUp - PlayerData.MinerExp);
        }
    }
}
