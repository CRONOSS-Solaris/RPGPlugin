using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RPGPlugin.Utils;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using Torch.Utils;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;
using static RPGPlugin.Roles;

namespace RPGPlugin.PointManagementSystem
{
    public sealed class MinerClass : ClassesBase
    {
        /// <inheritdoc />
        /// Point to your classConfig ExpRatio collection
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; } =
            new ObservableCollection<KeyValuePair<string, double>>();
        
        //test skill point system
        public override ObservableCollection<KeyValuePair<int, int>> SkillPoints { get; set; } =
            new ObservableCollection<KeyValuePair<int, int>>();

        private SkillPointCalculator _skillPointCalculator = new SkillPointCalculator();

        public override void init()
        {
            MinerConfig config = (MinerConfig)classConfigs["MinerConfig"];
            if (config != null)
                ExpRatio = config.ExpRatio;
                
                //skillpoint
                SkillPoints = config.SkillPoints;
            base.init();
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
                if (!_ProcessQueue.TryDequeue(out ExperienceAction queueData)) continue;
               
                // Get playerID from steamID
                ulong steamID = Sync.Players.TryGetSteamId(queueData.ownerID);
                
                // Make sure we have the value set, maybe not all owners want to reward all ore types mined.
                if (!xpTable.ContainsKey(queueData.subType)) continue;
                    
                // If player data no loaded yet, dont crash
                if (!PlayerManagers.ContainsKey(steamID)) continue;
                
                // If not a miner, no points given.
                if (PlayerManagers[steamID].GetRole() != "Miner") break;
                
                // shitteryCheck
                if (xpTable.Count == 0) break;
                if (!PlayerManagers.ContainsKey(steamID)) break;
                
                // This is calculated, stored in another concurrent list, then saved to file.  
                // we can await this to prevent and important process from running.
                
                await AddClassExp(steamID , xpTable[queueData.subType] * queueData.amount);
            }
            _queueInProcess = false;
        }

        /// <inheritdoc />
        public override int ExpToLevelUp(ulong steamID)
        {
            int expForLevelUp = (int)Math.Round(PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"].Item1 / 3.5 * 10000);
            return (int)Math.Round(expForLevelUp - PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"].Item2);
        }

        protected override Task AddClassExp(ulong steamID, double exp)
        {
            if (PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"].Item2 + exp >= ExpToLevelUp(steamID))
            {
                //skillpoint
                int level = PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"].Item1 + 1;
                int skillPoints = _skillPointCalculator.CalculateSkillPoints(level, SkillPoints.ToList());

                // Update SkillPoints in _PlayerData
                PlayerManagers[steamID]._PlayerData.SkillPoints += skillPoints;

                Tuple<int, double> UpdateData = new Tuple<int, double>(
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"].Item1 + 1,
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"].Item2 + exp - ExpToLevelUp(steamID)
                );

                PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"] = UpdateData;

                if (Instance.Config.BroadcastLevelUp)
                {
                    string name = MySession.Static.Players.TryGetIdentityNameFromSteamId(PlayerManagers[steamID]._PlayerData.SteamId);
                    ChatManager.SendMessageAsOther("Roles Manager", $"{name} is now a level {PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"].Item1} Miner!", Color.ForestGreen);
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
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"].Item1,
                    PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"].Item2 + exp - ExpToLevelUp(steamID)
                );
                PlayerManagers[steamID]._PlayerData.ClassInfo["Miner"] = UpdateData;
            }

            return Task.CompletedTask;
        }


        /// <inheritdoc />
        public override void Loaded() { }

        /// <inheritdoc />
        public override void Unloading() { }
    }

    // Here is a working example of how you can patch (suffix) a method in game.
    public static class DrillPatch
    {
        private static RPGPluginConfig Config => Roles.Instance.Config; 

        [ReflectedGetter(Name = "m_drillEntity")]
        public static Func<MyDrillBase, MyEntity> _getEntity;
		
        public static void Patch(PatchContext ctx) => ctx.GetPattern(typeof(MyDrillBase)
                .GetMethod("TryHarvestOreMaterial", BindingFlags.Instance | BindingFlags.NonPublic))
            .Suffixes.Add(typeof(DrillPatch).GetMethod("SuffixTryHarvestOreMaterial"));

        public static void SuffixTryHarvestOreMaterial
        (
            MyDrillBase __instance,
            MyVoxelMaterialDefinition material,
            Vector3D hitPosition,
            int removedAmount)
        {
            if (removedAmount == 0) return;
            MyCubeBlock drill = _getEntity(__instance) as MyCubeBlock;
            if (drill == null) return;
            
            
            roles["MinerClass"]._ProcessQueue.Enqueue(new ExperienceAction
            {
                ownerID = drill.OwnerId,
                subType = material.MinedOre,
                amount = removedAmount
            });
            
        }
    }
}