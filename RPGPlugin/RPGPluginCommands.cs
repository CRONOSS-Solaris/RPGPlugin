using Sandbox.Game.World;
using System.Text;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod.Messages;
using Torch.Mod;
using VRage.Game;
using VRage.Game.ModAPI;

namespace RPGPlugin
{
    [Category("r")]
    public class RolesCommands : CommandModule
    {


        public Roles Plugin => (Roles)Context.Plugin;

        [Command("setrole", "Set your role")]
        [Permission(MyPromoteLevel.None)]
        public async void SetRole(string roleName)
        {
            if (Context.Player == null)
            {
                Context.Respond("This is a player command only.");
                return;
            }

            if (!Roles.Instance.DelayFinished)
            {
                Context.Respond("Your profile has not been loaded yet.  Your actions are still logged and points applied when your profile is loaded.  Check again in a few minutes.");
            }

            if (!Roles.PlayerManagers.ContainsKey(Context.Player.IdentityId))
            {
                Context.Respond("Problem loading your profile.");
                return;
            }

            // Get player Manager
            switch (roleName.ToLower()) // No need for case sensitivity.
            {
                case "norole":
                    Roles.PlayerManagers[Context.Player.IdentityId]._PlayerData.SelectedRole = PlayerManager.FromRoles.NoRole;
                    await Roles.PlayerManagers[Context.Player.IdentityId].SavePlayerData();
                    Context.Respond("Your role has been updated to [No Role]");
                    break;

                case "miner":
                    Roles.PlayerManagers[Context.Player.IdentityId]._PlayerData.SelectedRole = PlayerManager.FromRoles.Miner;
                    await Roles.PlayerManagers[Context.Player.IdentityId].SavePlayerData();
                    Context.Respond("Your role has been updated to [Miner]");
                    break;

                case "warrior":
                    Roles.PlayerManagers[Context.Player.IdentityId]._PlayerData.SelectedRole = PlayerManager.FromRoles.Warrior;
                    await Roles.PlayerManagers[Context.Player.IdentityId].SavePlayerData();
                    Context.Respond("Your role has been updated to [Warrior]");
                    break;

                default:
                    Context.Respond("No role with that name found, check your spelling and try again.");
                    break;
            
            }
        }

        [Command("roles", "Displays the list of available roles and their descriptions.")]
        [Permission(MyPromoteLevel.None)]
        public void ListRoles()
        {
            if (Context.Player == null)
            {
                Context.Respond("This is a player command only.");
                return;
            }

            string message = "Miner - This role allows you to mine and gather resources more efficiently.\n" +
                             "Hunter - This role increases your damage against animals and improves your tracking abilities.\n" +
                             "Warrior - This role increases your damage against other players and gives you access to better weapons.";

            Plugin.SendRoles((IMyPlayer)Context.Player, "Available roles:", message);
        }



        [Command("stats", "Displays current level and exp needed for next level.")]
        [Permission(MyPromoteLevel.None)]
        public void Stats()
        {
            if (Context.Player == null)
            {
                Context.Respond("This is a player command.");
                return;
            }
            
            if (!Roles.PlayerManagers.ContainsKey(Context.Player.IdentityId))
            {
                Context.Respond("Problem loading your profile, please contact staff about this error if it continues.");
                return;
            }
            
            StringBuilder reply = new StringBuilder();
            reply.AppendLine("*** Information ***");
            reply.AppendLine("--------------------");
            reply.AppendLine("Miner:");
            reply.AppendLine($"Current level: {Roles.PlayerManagers[Context.Player.IdentityId]._PlayerData.MinerLevel.ToString()}.");
            reply.AppendLine($"Exp needed for next level: {Roles.PlayerManagers[Context.Player.IdentityId].ExpToLevelUp().ToString()}.");
            reply.AppendLine("--------------------");
            Context.Respond(reply.ToString());
            
        }
    }
}
