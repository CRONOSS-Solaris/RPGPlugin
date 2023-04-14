using System;
using System.Threading.Tasks;
using Sandbox.Game;
using VRageMath;
using RPGPlugin.Utils;
using Sandbox.Game.World;
using Sandbox.ModAPI;

namespace RPGPlugin
{
    public class PlayerManager
    {
        public PlayerData _PlayerData = new PlayerData();
        
        public enum FromRoles {NoRole, Miner, Warrior}

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

        private async Task LoadPlayerData(ulong steamId)
        {
            _PlayerData = await FileManager.LoadPlayerData(steamId);
        }

        public async Task<bool> SavePlayerData()
        {
            bool TaskResults = await FileManager.SavePlayerData(_PlayerData);
            return TaskResults;
        }
        
        public Task AddMinerExp(double exp)
        {

            if (_PlayerData.MinerExp + exp >= ExpToLevelUp())
            {
                _PlayerData.MinerLevel++;
                _PlayerData.MinerExp = Math.Round(_PlayerData.MinerExp + exp) - ExpToLevelUp();
                
                if (Roles.Instance.Config.BroadcastLevelUp)
                {
                    string name = MySession.Static.Players.TryGetIdentityNameFromSteamId(_PlayerData.SteamId);
                    Roles.ChatManager.SendMessageAsOther("Roles Manager", $"{name} is now a level {_PlayerData.MinerLevel} Miner!", Color.ForestGreen);
                }
                else
                {
                    MyVisualScriptLogicProvider.SendChatMessageColored("You have leveled up!!!", Color.Green, "Roles", _PlayerData.PlayerID);
                }
            }
            else
            {
                _PlayerData.MinerExp += exp;
            }

            return Task.CompletedTask;
        }

        public int ExpToLevelUp()
        {
            int expForLevelUp = (int)Math.Round(_PlayerData.MinerLevel / 3.5 * 10000);
            
            return (int)Math.Round(expForLevelUp - _PlayerData.MinerExp);
        }
    }
}
