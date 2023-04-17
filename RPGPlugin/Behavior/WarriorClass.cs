using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RPGPlugin.Utils;
using Sandbox.Game;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;
using static RPGPlugin.Roles;

namespace RPGPlugin.PointManagementSystem
{
    public class WarriorClass : ClassesBase
    {

        public void Init()
        {
            MyDamageInformation damageInformation = default;
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageHandler);
        }
            
        /// <inheritdoc />
        /// Point to your classConfig ExpRatio collection
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio
        {
            get => Instance.warriorConfig.ExpRatio;
            set => Instance.warriorConfig.ExpRatio = value;
        }

        /// <inheritdoc />
        protected override double _queueFrequency { get; set; } = 10;

        public void DamageHandler(object target, ref MyDamageInformation info)
        {
            // as fast as possible, get the needed information and send to the queue.
        }

        protected override async Task ProcessXpCollectedAsync()
        {
            // Make sure the process is not already running.
            if (_queueInProcess) return;

            // Not a lock but it acts like one while the time between queue process frequency is long enough.
            _queueInProcess = true;

            while (_ProcessQueue.Count > 0)
            {
                // Grab the first item from the queue, this will be the oldest item in the collection.
                if (!_ProcessQueue.TryDequeue(out ExperienceAction queueData)) continue;

                // Make sure we have the value set, maybe not all owners want to reward all actions.
                var expRatioDict = ExpRatio.ToDictionary(x => x.Key, x => x.Value);
                if (!expRatioDict.ContainsKey(queueData.subType)) continue;


                // If player data not loaded yet, don't crash
                if (!PlayerManagers.ContainsKey(queueData.ownerID)) continue;

                // If not a warrior, no points given.
                if (PlayerManagers[queueData.ownerID].GetRole() != PlayerManager.FromRoles.Warrior) break;

                // ShitteryCheck
                if (ExpRatio.Count == 0) break;
                if (!PlayerManagers.ContainsKey(queueData.ownerID)) break;

                // This is calculated, stored in another concurrent list, then saved to file.
                // we can await this to prevent and important process from running.
                await AddClassExp(queueData.ownerID, expRatioDict[queueData.subType]);
            }
            _queueInProcess = false;
        }

        private double CalculateExpFromKills(long playerId)
        {
            double exp = 0;
            // Implement logic for calculating exp from killing enemy players
            return exp;
        }

        private double CalculateExpFromDestroyedBlocks(long playerId)
        {
            double exp = 0;
            // Implement logic for calculating exp from destroying enemy blocks
            return exp;
        }

        public override int ExpToLevelUp(long id)
        {
            throw new System.NotImplementedException();
        }

        protected override Task AddClassExp(long id, double exp)
        {
            if (PlayerManagers[id]._PlayerData.WarriorExp + exp >= ExpToLevelUp(id))
            {
                PlayerManagers[id]._PlayerData.WarriorLevel++;
                PlayerManagers[id]._PlayerData.WarriorExp =
                    Math.Round(PlayerManagers[id]._PlayerData.WarriorExp + exp) - ExpToLevelUp(id);

                if (Instance.Config.BroadcastLevelUp)
                {
                    string name =
                        MySession.Static.Players.TryGetIdentityNameFromSteamId(PlayerManagers[id]._PlayerData.SteamId);
                    ChatManager.SendMessageAsOther("Roles Manager",
                        $"{name} is now a level {PlayerManagers[id]._PlayerData.WarriorLevel} Warrior!", Color.ForestGreen);
                }
                else
                {
                    MyVisualScriptLogicProvider.SendChatMessageColored("You have leveled up!!!", Color.Green, "Roles",
                        PlayerManagers[id]._PlayerData.PlayerID);
                }
            }
            else
            {
                PlayerManagers[id]._PlayerData.WarriorExp += exp;
            }

            return Task.CompletedTask;
        }
        //WarriorConfig.LoadWarriorConfig() in the appropriate place, just as you did for MinerConfig.LoadMinerConfig().
        
    }
}
