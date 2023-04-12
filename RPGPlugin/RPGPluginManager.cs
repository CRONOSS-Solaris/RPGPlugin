using System;
using System.Globalization;
using System.Threading.Tasks;
using Sandbox.Game;
using VRageMath;
using RPGPlugin.Utils;

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
                MyVisualScriptLogicProvider.SendChatMessageColored("You have leveled up!!!", Color.Green, "Roles", _PlayerData.PlayerID);
            }
            else
            {
                _PlayerData.MinerExp += exp;
                Roles.Log.Warn($"Your EXP = {_PlayerData.MinerExp.ToString(CultureInfo.InvariantCulture)}");
            }

            return Task.CompletedTask;
        }

        public int ExpToLevelUp()
        {
            int expForLevelUp = (int)Math.Round(_PlayerData.MinerLevel * (32.4 + _PlayerData.MinerLevel));
            return (int)Math.Round(expForLevelUp - _PlayerData.MinerExp);
        }
    }
}
