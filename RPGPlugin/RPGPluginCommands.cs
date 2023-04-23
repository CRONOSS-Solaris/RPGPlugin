using System;
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
            for (int index = Roles.Instance.Config.RegisteredRoles.Count - 1; index >= 0; index--)
            {
                if (roleName != Roles.Instance.Config.RegisteredRoles[index].Item1) continue;
                
                Roles.PlayerManagers[Context.Player.SteamUserId].SetRole(roleName);
                await Roles.PlayerManagers[Context.Player.SteamUserId].SavePlayerData();
                Context.Respond($"Your role has been updated to [{roleName}]");
            }
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
            
            StringBuilder reply = new StringBuilder();
            reply.AppendLine("*** Information ***");
            reply.AppendLine("—————————————————————————————");
            reply.AppendLine($"Current Role: {Roles.PlayerManagers[Context.Player.SteamUserId].GetRole()}");
            reply.AppendLine("—————————————————————————————");
            foreach (Tuple<string,string> role in Roles.Instance.Config.RegisteredRoles)
            {
                reply.AppendLine($"{role.Item1}:");
                reply.AppendLine($"Current level: {Roles.PlayerManagers[Context.Player.SteamUserId]._PlayerData.ClassInfo[role.Item1].Item1}.");
                reply.AppendLine($"Exp needed for next level: {Roles.roles[role.Item1 + "Class"].ExpToLevelUp(Context.Player.SteamUserId).ToString()}.");
                reply.AppendLine("—————————————————————————————");
            }
            Context.Respond(reply.ToString());
        }
    }
}
