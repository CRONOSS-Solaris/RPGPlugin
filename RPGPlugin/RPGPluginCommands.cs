using System.IO;
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

            if (config.RolesList.Contains(roleName))
            {
                // save player's data
                ulong steamId = player.SteamUserId;
                string dataPath = Path.Combine(Plugin.StoragePath, "RPGPlugin", "Player Data");
                string filePath = Path.Combine(dataPath, $"{steamId}.xml");

                // create the player's data if it doesn't exist
                var serializer = new XmlSerializer(typeof(PlayerData));
                PlayerData playerData;
                if (!File.Exists(filePath))
                {
                    playerData = new PlayerData(steamId);
                }
                else
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        playerData = (PlayerData)serializer.Deserialize(reader);
                    }
                }

                // set the selected role and save the player's data
                if (playerData.SelectedRole == null)
                {
                    playerData.SelectedRole = roleName;
                    using (var writer = new StreamWriter(filePath))
                    {
                        serializer.Serialize(writer, playerData);
                    }
                    Context.Respond($"Role set to {roleName} for {player.DisplayName}.");
                }
                else
                {
                    Context.Respond($"{player.DisplayName}, you can only set your role once.");
                }
            }
            else
            {
                Context.Respond($"Invalid role {roleName}.");
            }
        }


        [Command("roles", "Displays the list of available roles and their descriptions.")]
        [Permission(MyPromoteLevel.None)]
        public void ListRoles()
        {
            var message = "Available roles:\n";
            message += "Miner - This role allows you to mine and gather resources more efficiently.\n";
            message += "Hunter - This role increases your damage against animals and improves your tracking abilities.\n";
            message += "Warrior - This role increases your damage against other players and gives you access to better weapons.\n";

            Context.Respond(message);
        }

        //[Command("stats", "Displays current level and exp needed for next level.")]
        //[Permission(MyPromoteLevel.None)]
        //public void Stats()
        //{
            //var message = "Informations:\n";
            //message += $"Current level: .\n";
            //message += $"Exp needed for next level: .\n";

            //Context.Respond(message);
        //}
    }
}
