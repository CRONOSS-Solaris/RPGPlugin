using System;
using System.Linq;
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
        public void SetRole(string roleName)
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

            // Check if the role is valid
            if (!Roles.Instance.Config.RegisteredRoles.Any(r => r.Item1.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
            {
                Context.Respond($"Invalid role. Please choose from the following: {string.Join(", ", Roles.Instance.Config.RegisteredRoles.Select(r => r.Item1))}");
                return;
            }

            // Set the role
            Roles.PlayerManagers[Context.Player.SteamUserId].SetRole(roleName);

            // Initialize the ClassInfo dictionary for the selected role if not already present
            if (!Roles.PlayerManagers[Context.Player.SteamUserId]._PlayerData.ClassInfo.ContainsKey(roleName))
            {
                Roles.PlayerManagers[Context.Player.SteamUserId]._PlayerData.ClassInfo[roleName] = new Tuple<int, double>(1, 0);
            }

            // Save player data
            Roles.PlayerManagers[Context.Player.SteamUserId].SavePlayerData().Wait(); // Preferably use async/await properly as in your original code, but it requires changing the method signature and related code

            Context.Respond($"Your role has been updated to [{roleName}]");
        }


        [Command("roles", "Displays the list of available roles and their descriptions.")]
        [Permission(MyPromoteLevel.None)]
        public void ListRoles()
        {
            StringBuilder message = new StringBuilder();
            foreach (Tuple<string,string> role in Roles.Instance.Config.RegisteredRoles)
                message.AppendLine($"{role.Item1} -> {role.Item2}");
            
            Context.Respond(message.ToString());
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
                Context.Respond("Players data will not be loaded shortly after restarts to allow the server to stabilize.");
                return;
            }

            if (!Roles.PlayerManagers.ContainsKey(Context.Player.SteamUserId))
            {
                Context.Respond("Problem loading your profile or your profile has not been loaded yet. Please contact staff about this error if it continues.");
                return;
            }

            // Check if the player has a role
            var currentPlayerRole = Roles.PlayerManagers[Context.Player.SteamUserId].GetRole();
            if (string.IsNullOrEmpty(currentPlayerRole))
            {
                Context.Respond("You have not selected a role yet. Please choose a role using the '!r setrole [RoleName]' command.");
                return;
            }

            StringBuilder reply = new StringBuilder();
            reply.AppendLine("*** Information ***");
            reply.AppendLine("—————————————————————————————");
            reply.AppendLine($"Current Role: {currentPlayerRole}");
            reply.AppendLine("—————————————————————————————");
            foreach (Tuple<string, string> role in Roles.Instance.Config.RegisteredRoles)
            {
                if (!Roles.PlayerManagers[Context.Player.SteamUserId]._PlayerData.ClassInfo.ContainsKey(role.Item1))
                {
                    // If the player does not have the role yet, continue the loop
                    continue;
                }

                reply.AppendLine($"{role.Item1}:");
                reply.AppendLine($"Current level: {Roles.PlayerManagers[Context.Player.SteamUserId]._PlayerData.ClassInfo[role.Item1].Item1}.");
                reply.AppendLine($"Exp needed for next level: {Roles.roles[role.Item1 + "Class"].ExpToLevelUp(Context.Player.SteamUserId).ToString()}.");
                reply.AppendLine("—————————————————————————————");
            }

            Context.Respond(reply.ToString());
        }
    }
}
