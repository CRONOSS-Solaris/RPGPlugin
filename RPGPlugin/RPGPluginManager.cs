using System;
using System.Threading.Tasks;
using Sandbox.Game;
using VRageMath;
using RPGPlugin.Utils;
using Sandbox.Game.World;

namespace RPGPlugin
{
    public sealed class PlayerManager
    {
        public PlayerData _PlayerData { get; set; } = new PlayerData();
        
        public enum FromRoles {NoRole, Miner, Warrior, Hunter}

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

        public int GetLevel()
        {
            return _PlayerData.MinerLevel;
        }

        public long GetPlayerID()
        {
            return _PlayerData.PlayerID;
        }

        public ulong GetSteamID()
        {
            return _PlayerData.SteamId;
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
    }
}
