using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
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
            var player = Context.Player;
            var config = Plugin.Config;

            // Get player Manager
            for (int index = Roles.PlayerManagers.Count - 1; index >= 0; index--)
            {
                if (Roles.PlayerManagers[index].PlayerData.SteamId != Context.Player.SteamUserId) continue;
                
                RoleManager manager = Roles.PlayerManagers[index];
                switch (roleName.ToLower()) // No need for case sensitivity.
                {
                    case "norole":
                        manager.PlayerData.SelectedRole = RoleManager.FromRoles.NoRole;
                        manager.SavePlayerData();
                        Context.Respond("Your role has been updated to [No Role]");
                        break;

                    case "miner":
                        manager.PlayerData.SelectedRole = RoleManager.FromRoles.Miner;
                        manager.SavePlayerData();
                        Context.Respond("Your role has been updated to [Miner]");
                        break;

                    case "warrior":
                        manager.PlayerData.SelectedRole = RoleManager.FromRoles.Warrior;
                        manager.SavePlayerData();
                        Context.Respond("Your role has been updated to [Warrior]");
                        break;

                    default:
                        Context.Respond("No role with that name found, check your spelling and try again.");
                        break;
                }
            }
        }

        [Command("roles", "Displays the list of available roles and their descriptions.")]
        [Permission(MyPromoteLevel.None)]
        public void ListRoles()
        {
            StringBuilder reply = new StringBuilder();
            reply.AppendLine("Available roles:");
            reply.AppendLine("Miner - This role allows you to mine and gather resources more efficiently.");
            reply.AppendLine("Hunter - This role increases your damage against animals and improves your tracking abilities.");
            reply.AppendLine("Warrior - This role increases your damage against other players and gives you access to better weapons.");

            Context.Respond(reply.ToString());
        }

        [Command("stats", "Displays current level and exp needed for next level.")]
        [Permission(MyPromoteLevel.None)]
        public void Stats()
        {
            for (int index = Roles.PlayerManagers.Count - 1; index >= 0; index--)
            {
                if (Roles.PlayerManagers[index].PlayerData.SteamId != Context.Player.SteamUserId) continue;
                RoleManager manager = Roles.PlayerManagers[index];
                
                int expNeededForNextLevel = manager.PlayerData.ExpPerLevel[manager.PlayerData.Level]; // Obliczanie wymaganego doświadczenia na podstawie listy

                StringBuilder reply = new StringBuilder();
                reply.AppendLine("Information:");
                reply.AppendLine($"Current level: {manager.PlayerData.Level}.");
                reply.AppendLine($"Exp needed for next level: {expNeededForNextLevel - manager.PlayerData.Exp}.");

                Context.Respond(reply.ToString());
            }
        }
    }
}
