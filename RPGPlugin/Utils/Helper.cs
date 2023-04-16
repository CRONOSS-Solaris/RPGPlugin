using Torch.Mod;
using Torch.Mod.Messages;
using VRage.Game.ModAPI;

namespace RPGPlugin.Utils
{
    public static class Helper
    {
        // MOTD command style
        public static void SendRoleData(IMyPlayer player, string header, string message)
        {
            /*
             *    Keen doesn't accept \r, \n, \r\n, or Environment.NewLine()
             *    on a persistence basis.  Easier to use StringBuilder()
             *    plus StringBuilder() is faster and easier on the system.
             */  
            var msg = new DialogMessage("RPGPLUGIN", header, message);
            ModCommunication.SendMessageTo(msg, player.SteamUserId);
        }
    }
}