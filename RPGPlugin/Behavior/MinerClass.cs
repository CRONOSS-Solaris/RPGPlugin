using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RPGPlugin.Utils;
using Sandbox.Game;
using Sandbox.Game.World;
using VRageMath;
using static RPGPlugin.Roles;

namespace RPGPlugin.PointManagementSystem
{
    public sealed class MinerClass : ClassesBase
    {
        /// <inheritdoc />
        /// Point to your classConfig ExpRatio collection
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio
        {
            get => Instance.minerConfig.ExpRatio;
            set => Instance.minerConfig.ExpRatio = value;
        }

        /// <inheritdoc />
        protected override double _queueFrequency { get; set; } = 10;

        /// <inheritdoc />
        protected override async Task ProcessXpCollectedAsync()
        {
            // Make sure the process is not already running.
            if (_queueInProcess) return;

            // Not a lock but it acts like one while the time between queue process frequency is long enough.
            _queueInProcess = true;
            while (_ProcessQueue.Count > 0)
            {
                // Grab the first item from the queue, this will be the oldest item in the collection.
                if (!_ProcessQueue.TryDequeue(out CollectedOre queueData)) continue;
                
                // Make sure we have the value set, maybe not all owners want to reward all ore types mined.
                if (!xpTable.ContainsKey(queueData.subType)) continue;
                    
                // If player data no loaded yet, dont crash
                if (!PlayerManagers.ContainsKey(queueData.ownerID)) continue;
                
                // If not a miner, no points given.
                if (PlayerManagers[queueData.ownerID].GetRole() != PlayerManager.FromRoles.Miner) break;
                
                // shitteryCheck
                if (xpTable.Count == 0) break;
                if (!PlayerManagers.ContainsKey(queueData.ownerID)) break;
                
                // This is calculated, stored in another concurrent list, then saved to file.  
                // we can await this to prevent and important process from running.
                
                await AddClassExp(queueData.ownerID , xpTable[queueData.subType] * queueData.amount);
            }
            _queueInProcess = false;
        }

        /// <inheritdoc />
        public override int ExpToLevelUp(long id)
        {
            int expForLevelUp = (int)Math.Round(PlayerManagers[id]._PlayerData.MinerLevel / 3.5 * 10000);
            return (int)Math.Round(expForLevelUp - PlayerManagers[id]._PlayerData.MinerExp);
        }

        /// <inheritdoc />
        protected override Task AddClassExp(long id, double exp)
        {
            if (PlayerManagers[id]._PlayerData.MinerExp + exp >= ExpToLevelUp(id))
            {
                PlayerManagers[id]._PlayerData.MinerLevel++;
                PlayerManagers[id]._PlayerData.MinerExp =
                    Math.Round(PlayerManagers[id]._PlayerData.MinerExp + exp) - ExpToLevelUp(id);

                if (Instance.Config.BroadcastLevelUp)
                {
                    string name =
                        MySession.Static.Players.TryGetIdentityNameFromSteamId(PlayerManagers[id]._PlayerData.SteamId);
                    ChatManager.SendMessageAsOther("Roles Manager",
                        $"{name} is now a level {PlayerManagers[id]._PlayerData.MinerLevel} Miner!", Color.ForestGreen);
                }
                else
                {
                    MyVisualScriptLogicProvider.SendChatMessageColored("You have leveled up!!!", Color.Green, "Roles",
                        PlayerManagers[id]._PlayerData.PlayerID);
                }
            }
            else
            {
                PlayerManagers[id]._PlayerData.MinerExp += exp;
            }

            return Task.CompletedTask;
        }
    }
}