using System;
using System.Reflection;
using RPGPlugin.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.ViewModels;
using Sandbox.Game.Weapons;
using Torch.Managers.PatchManager;
using Torch.Utils;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;

namespace RPGPlugin.Patches
{
    public static class DrillPatch
    {
        private static RPGPluginConfig Config => Roles.Instance.Config; 

        [ReflectedGetter(Name = "m_drillEntity")]
        public static Func<MyDrillBase, MyEntity> _getEntity;
		
        public static void Patch(PatchContext ctx) => ctx.GetPattern((MethodBase)typeof(MyDrillBase)
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
            
            
            Roles.Instance.PointsManager.MinerProtocol._ProcessQueue.Enqueue(new CollectedOre
            {
                ownerID = drill.OwnerId,
                subType = material.MinedOre,
                amount = removedAmount
            });
            
        }
    }
}