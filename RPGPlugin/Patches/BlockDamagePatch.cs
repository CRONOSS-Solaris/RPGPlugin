using RPGPlugin.RPGPlugin.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace RPGPlugin.Patches
{
    public static class BlockDamagePatch
    {
        private static RPGPluginConfig Config => Roles.Instance.Config;
        private static Roles Instance => Roles.Instance;

        public static void Init()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, BeforeDamageHandler);
        }

        private static void BeforeDamageHandler(object targetEntity, ref MyDamageInformation info)
        {
            // Check if the target is a block and the attacker is a player
            if (targetEntity is MySlimBlock block && info.AttackerId != 0)
            {
                // Get the player identity
                var playerIdentity = MySession.Static.Players.TryGetIdentity(info.AttackerId);

                if (playerIdentity == null) return;

                // Get the player's role
                var playerRole = Roles.PlayerManagers[info.AttackerId].GetRole();

                // Check if the player's role is Warrior
                if (playerRole == PlayerManager.FromRoles.Warrior)
                {
                    // Add the destroyed block to the process queue for the warrior class
                    Roles.Instance.PointsManager.WarriorProtocol._WarriorProcessQueue.Enqueue(new WarriorActionData
                    {
                        OwnerID = info.AttackerId,
                        ActionType = "DestroyedBlock"
                    });
                }
            }
        }
    }
}