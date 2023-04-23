using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RPGPlugin.Utils;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
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
    public class HunterClass : ClassesBase
    {
        /// <inheritdoc />
        /// Point to your classConfig ExpRatio collection
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; } =
            new ObservableCollection<KeyValuePair<string, double>>();

        //test skill point system
        public override ObservableCollection<KeyValuePair<int, int>> SkillPoints { get; set; } =
            new ObservableCollection<KeyValuePair<int, int>>();

        /// <inheritdoc /> 
        public override void init()
        {
            HunterConfig config = (HunterConfig)classConfigs["HunterConfig"];
            if (config != null)
                ExpRatio = config.ExpRatio;
            base.init();
        }

        /// <inheritdoc />
        protected override double _queueFrequency { get; set; } = 10;
        
        private void OnCharacterDamaged(object target, ref MyDamageInformation damageInfo)
        {
            if (target is MyCharacter character)
            {
                if (character.IsDead && (character.DisplayName == "Wolf" || character.DisplayName == "Spider"))
                {
                    double exp = CalculateExpFromAnimalKills(damageInfo.AttackerId, character.DisplayName);

                    _ProcessQueue.Enqueue(new ExperienceAction
                    {
                        ownerID = damageInfo.AttackerId,
                        subType = character.DisplayName,
                        amount = exp
                    });
                }
            }
        }

        private void BlockDestroyedHandler(object target, ref MyDamageInformation info)
        {
            if (target is MyCubeBlock block)
            {
                if (block.IDModule.GetUserRelationToOwner(info.AttackerId) != MyRelationsBetweenPlayerAndBlock.Friends)
                {
                    if (IsOwnedByNPC(block))
                    {
                        switch (block.CubeGrid.GridSizeEnum)
                        {
                            case MyCubeSize.Small:
                                _ProcessQueue.Enqueue(new ExperienceAction
                                {
                                    ownerID = info.AttackerId,
                                    subType = "EnemySmallBlock",
                                    amount = info.Amount
                                });
                                break;
                            case MyCubeSize.Large:
                                _ProcessQueue.Enqueue(new ExperienceAction
                                {
                                    ownerID = info.AttackerId,
                                    subType = "EnemyLargeBlock",
                                    amount = info.Amount
                                });
                                break;
                        }
                    }
                }
            }
        }
        
        private bool IsOwnedByNPC(MyCubeBlock block)
        {
            long ownerId = block.OwnerId;
            ulong identityId = (ulong)Math.Abs(ownerId);
            MyPlayer player = null;
            MySession.Static.Players.TryGetPlayerById(new MyPlayer.PlayerId(identityId), out player);
            return player?.IsBot ?? false;
        }

        /// <inheritdoc />
        protected override async Task ProcessXpCollectedAsync()
        {
            if (_queueInProcess) return;

            _queueInProcess = true;

            while (_ProcessQueue.Count > 0)
            {
                if (!_ProcessQueue.TryDequeue(out ExperienceAction queueData)) continue;

                ulong steamID = Sync.Players.TryGetSteamId(queueData.ownerID);

                Dictionary<string, double> expRatioDict = ExpRatio.ToDictionary(x => x.Key, x => x.Value);
                if (!expRatioDict.ContainsKey(queueData.subType)) continue;

                if (!PlayerManagers.ContainsKey(steamID)) continue;

                if (PlayerManagers[steamID].GetRole() != "Hunter") continue;

                if (xpTable.Count == 0) break;
                if (!PlayerManagers.ContainsKey(steamID)) continue;

                await AddClassExp(steamID, expRatioDict[queueData.subType]);
            }
            _queueInProcess = false;
        }

        // All damage calculations should be part of the same method.  Use a switch statement for best performance.
        private double CalculateExpFromAnimalKills(long playerId, string animalType)
        {
            double exp = 0;

            // Use the ExpRatio dictionary to get the experience value for the given animal type
            Dictionary<string, double> expRatioDict = ExpRatio.ToDictionary(x => x.Key, x => x.Value);
            if (expRatioDict.ContainsKey(animalType))
            {
                exp = expRatioDict[animalType];
            }

            return exp;
        }
        
        // All damage calculations should be part of the same method.  Use a switch statement for best performance.
        private double CalculateExpFromDestroyedBlocks(long playerId)
        {
            double exp = 0;
            // Implement logic for calculating exp from destroying enemy blocks (of NPC)
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
            if (PlayerManagers[steamID]._PlayerData.ClassInfo["Hunter"].Item2 + exp >= ExpToLevelUp(steamID))
            {
                Tuple<int, double> UpdateData = new Tuple<int, double>(
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Hunter"].Item1 + 1,
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Hunter"].Item2 + exp - ExpToLevelUp(steamID)
                );

                PlayerManagers[steamID]._PlayerData.ClassInfo["Hunter"] = UpdateData;

                if (Instance.Config.BroadcastLevelUp)
                {
                    string name = MySession.Static.Players.TryGetIdentityNameFromSteamId(PlayerManagers[steamID]._PlayerData.SteamId);
                    ChatManager.SendMessageAsOther("Roles Manager", $"{name} is now a level {PlayerManagers[steamID]._PlayerData.ClassInfo["Hunter"]} Hunter!", Color.ForestGreen);
                }
            }
            else
            {
                Tuple<int, double> UpdateData = new Tuple<int, double>(
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Hunter"].Item1,
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Hunter"].Item2 + exp - ExpToLevelUp(steamID)
                );
                PlayerManagers[steamID]._PlayerData.ClassInfo["Hunter"] = UpdateData;
            }

            return Task.CompletedTask;
        }
        
        /// <inheritdoc /> 
        public override void Loaded()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, OnCharacterDamaged);
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, BlockDestroyedHandler);
        }

        /// <inheritdoc /> 
        public override void Unloading()
        {
            
        }
    }
}
