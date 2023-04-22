using System.Text;
using RPGPlugin.Utils;
using Torch.Commands;
using Torch.Commands.Permissions;
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
                Context.Respond("Players data will not be loaded immediately after restarts to allow the server to stabilize.");
                return;
            }

            if (!Roles.PlayerManagers.ContainsKey(Context.Player.SteamUserId))
            {
                Context.Respond("Problem loading your profile.");
                return;
            }

            // Get player Manager
            switch (roleName.ToLower()) // No need for case sensitivity.
            {
                case "norole":
                    Roles.PlayerManagers[Context.Player.SteamUserId].SetRole(PlayerManager.FromRoles.NoRole);
                    await Roles.PlayerManagers[Context.Player.SteamUserId].SavePlayerData();
                    Context.Respond("Your role has been updated to [No Role]");
                    break;

                case "miner":
                    Roles.PlayerManagers[Context.Player.SteamUserId].SetRole(PlayerManager.FromRoles.Miner);
                    await Roles.PlayerManagers[Context.Player.SteamUserId].SavePlayerData();
                    Context.Respond("Your role has been updated to [Miner]");
                    break;

                case "warrior":
                    Roles.PlayerManagers[Context.Player.SteamUserId].SetRole(PlayerManager.FromRoles.Warrior);
                    await Roles.PlayerManagers[Context.Player.SteamUserId].SavePlayerData();
                    Context.Respond("Your role has been updated to [Warrior]");
                    break;
                
                case "hunter":
                    Roles.PlayerManagers[Context.Player.SteamUserId].SetRole(PlayerManager.FromRoles.Hunter);
                    await Roles.PlayerManagers[Context.Player.SteamUserId].SavePlayerData();
                    Context.Respond("Your role has been updated to [Hunter]");
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
            
            if (!Roles.Instance.DelayFinished)
            {
                Context.Respond("Players data will not be loaded immediately after restarts to allow the server to stabilize.");
                return;
            }

            StringBuilder message = new StringBuilder();
            message.AppendLine("Miner - This role allows you to mine and gather resources more efficiently.");
            message.AppendLine();
            message.AppendLine("Hunter - This role increases your damage against animals and improves your tracking abilities.");
            message.AppendLine();
            message.AppendLine("Warrior - This role increases your damage against other players and gives you access to better weapons.");

            Helper.SendRoleData(Context.Player, "Available roles:", message.ToString());
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

            if (!Roles.Instance.DelayFinished)
            {
                Context.Respond("Players data will not be loaded immediately after restarts to allow the server to stabilize.");
                return;
            }
            
            if (!Roles.PlayerManagers.ContainsKey(Context.Player.SteamUserId))
            {
                Context.Respond("Problem loading your profile or your profile has not been loaded yet. Please contact staff about this error if it continues.");
                return;
            }
            
            StringBuilder reply = new StringBuilder();
            reply.AppendLine("*** Information ***");
            reply.AppendLine("--------------------");
            reply.AppendLine("Miner:");
            reply.AppendLine($"Current level: {Roles.PlayerManagers[Context.Player.SteamUserId].GetLevel().ToString()}.");
            reply.AppendLine($"Exp needed for next level: {Roles.roles["MinerClass"].ExpToLevelUp(Context.Player.SteamUserId).ToString()}.");
            reply.AppendLine("--------------------");
            Context.Respond(reply.ToString());
            
        }
    }
}
