using System;
using System.Reflection;
using RPGPlugin.RPGPlugin.Utils;
using RPGPlugin.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using Torch.Utils;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace RPGPlugin.Patches
{
    public static class BlockDamagePatch
    {
        private static RPGPluginConfig Config => Roles.Instance.Config;

        public static void Patch(PatchContext ctx) => ctx.GetPattern((MethodBase)typeof(MySlimBlock)
                .GetMethod("DoDamage", BindingFlags.Instance | BindingFlags.Public))
            .Suffixes.Add(typeof(BlockDamagePatch).GetMethod("SuffixDoDamage"));

        public static void SuffixDoDamage
        (
            MySlimBlock __instance,
            float damage,
            MyStringHash damageType,
            bool sync,
            MyHitInfo? hitInfo,
            long PlayerID
        )
        {
            // Check if the block was destroyed and the attacker is a player
            if (__instance.IsDestroyed && PlayerID != 0)
            {
                // Get the player identity
                var playerIdentity = MySession.Static.Players.TryGetIdentity(PlayerID);

                if (playerIdentity == null) return;

                // Get the player's role
                var playerRole = Roles.PlayerManagers[PlayerID].GetRole();

                // Check if the player's role is Warrior
                if (playerRole == PlayerManager.FromRoles.Warrior)
                {
                    // Add the destroyed block to the process queue for the warrior class
                    Roles.Instance.PointsManager.WarriorProtocol._WarriorProcessQueue.Enqueue(new WarriorActionData
                    {
                        OwnerID = PlayerID,
                        ActionType = "DestroyedBlock"
                    });
                }
            }
        }

    }
}