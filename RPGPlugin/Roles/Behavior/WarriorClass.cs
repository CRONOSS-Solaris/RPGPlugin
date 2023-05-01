using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RPGPlugin.Utils;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using static RPGPlugin.Roles;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;
using IMySlimBlock = VRage.Game.ModAPI.Ingame.IMySlimBlock;

namespace RPGPlugin.PointManagementSystem
{
    public class WarriorClass : ClassesBase
    {
        /// <inheritdoc />
        /// Point to your classConfig ExpRatio collection
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; } =
            new ObservableCollection<KeyValuePair<string, double>>();

        //test skill point system
        public override ObservableCollection<KeyValuePair<int, int>> SkillPoints { get; set; } =
            new ObservableCollection<KeyValuePair<int, int>>();

        private SkillPointCalculator _skillPointCalculator = new SkillPointCalculator();

        /// <inheritdoc /> 
        public override void init()
        {
            WarriorConfig config = (WarriorConfig)classConfigs["WarriorConfig"];
            if (config != null)
                ExpRatio = config.ExpRatio;
            base.init();
        }

        /// <inheritdoc />
        protected override double _queueFrequency { get; set; } = 10;

        private void DamageHandler(object target, ref MyDamageInformation info)
        {
            // Deformation damage can be from ramming such as torpedo grids, or a warship hitting another.
            // Will do a sample here.  Be warned... This requires allot of work and will become a bit long... :)

            if (target is MyCubeBlock block)
            {
                // Get relation between grid and attacker
                if (block.IDModule.GetUserRelationToOwner(info.AttackerId) != MyRelationsBetweenPlayerAndBlock.Friends)
                {
                    switch (block.CubeGrid.GridSizeEnum)
                    {
                        case MyCubeSize.Small:
                            _ProcessQueue.Enqueue( new ExperienceAction
                            {
                                ownerID = info.AttackerId,
                                subType = "EnemySmallBlock",
                                amount = info.Amount // This gives points per point of damage.
                            });
                            break;
                        case MyCubeSize.Large:
                            _ProcessQueue.Enqueue( new ExperienceAction
                            {
                                ownerID = info.AttackerId,
                                subType = "EnemyLargeBlock",
                                amount = info.Amount // This gives points per point of damage.
                            });
                            break;
                    }
                }
            }
        }

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
                if (!_ProcessQueue.TryDequeue(out ExperienceAction queueData)) continue;
                
                // Get playerID from steamID
                ulong steamID = Sync.Players.TryGetSteamId(queueData.ownerID);

                // Make sure we have the value set, maybe not all owners want to reward all actions.
                Dictionary<string, double> expRatioDict = ExpRatio.ToDictionary(x => x.Key, x => x.Value);
                if (!expRatioDict.ContainsKey(queueData.subType)) continue;

                // If player data not loaded yet, don't crash
                if (!PlayerManagers.ContainsKey(steamID)) continue;

                // If not a warrior, no points given.
                if (PlayerManagers[steamID].GetRole() != "Warrior") break;

                // ShitteryCheck
                if (ExpRatio.Count == 0) break;
                if (!PlayerManagers.ContainsKey(steamID)) break;

                // This is calculated, stored in another concurrent list, then saved to file.
                // we can await this to prevent and important process from running.
                await AddClassExp(steamID, expRatioDict[queueData.subType]);
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

        /// <inheritdoc /> 
        public override int ExpToLevelUp(ulong steamID)
        {
            return 1; // Temp placeholder.
        }

        /// <inheritdoc /> 
        protected override Task AddClassExp(ulong steamID, double exp)
        {
            if (PlayerManagers[steamID]._PlayerData.ClassInfo["Warrior"].Item2 + exp >= ExpToLevelUp(steamID))
            {
                //skillpoint
                int level = PlayerManagers[steamID]._PlayerData.ClassInfo["Warrior"].Item1 + 1;
                int skillPoints = _skillPointCalculator.CalculateSkillPoints(level, SkillPoints.ToList());

                // Update SkillPoints in _PlayerData
                PlayerManagers[steamID]._PlayerData.SkillPoints += skillPoints;

                Tuple<int, double> UpdateData = new Tuple<int, double>(
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Warrior"].Item1 + 1,
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Warrior"].Item2 + exp - ExpToLevelUp(steamID)
                );

                PlayerManagers[steamID]._PlayerData.ClassInfo["Warrior"] = UpdateData;

                if (Instance.Config.BroadcastLevelUp)
                {
                    string name =
                        MySession.Static.Players.TryGetIdentityNameFromSteamId(PlayerManagers[steamID]._PlayerData.SteamId);
                    ChatManager.SendMessageAsOther("Roles Manager",
                        $"{name} is now a level {PlayerManagers[steamID]._PlayerData.ClassInfo["Warrior"].Item1} Warrior!", Color.ForestGreen);
                }
                else
                {
                    MyVisualScriptLogicProvider.SendChatMessageColored("You have leveled up!!!", Color.Green, "Roles",
                        PlayerManagers[steamID]._PlayerData.PlayerID);
                }

                // Add the skill points message here.
                string skillPointsMessage = $"You received {skillPoints} skill points for advancing to {level}.";
                MyVisualScriptLogicProvider.SendChatMessage(skillPointsMessage, "Roles", PlayerManagers[steamID]._PlayerData.PlayerID);
            }
            else
            {
                Tuple<int, double> UpdateData = new Tuple<int, double>(
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Warrior"].Item1,
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Warrior"].Item2 + exp - ExpToLevelUp(steamID)
                );
                PlayerManagers[steamID]._PlayerData.ClassInfo["Warrior"] = UpdateData;
            }

            return Task.CompletedTask;
        }
        
        /// <inheritdoc /> 
        public override void Loaded()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageHandler);
        }

        /// <inheritdoc /> 
        public override void Unloading()
        {
            
        }
    }
}
