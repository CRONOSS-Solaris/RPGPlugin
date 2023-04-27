using System;
using System.Collections.Generic;
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

        public static List<Tuple<string,string,string,double>> FromRoles = new List<Tuple<string, string, string, double>>();  // Role, Description

        public async void InitAsync(ulong steamId)
        {
            await LoadPlayerData(steamId);
        }

        public async void SetRole(string role)
        {
            _PlayerData.SelectedRole = role;
            await SavePlayerData();
        }

        public string GetRole()
        {
            return _PlayerData.SelectedRole;
        }

        public int GetLevel()
        {
            return _PlayerData.ClassInfo[GetRole()].Item1;
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
