using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RPGPlugin.PointManagementSystem;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage;
using VRage.Game.ModAPI;

namespace RPGPlugin.Utils
{
    public static class Helper
    {
        // MOTD command style
        public static void SendRoleData(IMyPlayer player, string header, string message)
        {
            /*    Keen doesn't accept \r, \n, \r\n, or Environment.NewLine()
             *    on a persistence basis.  Easier to use StringBuilder()
             *    plus StringBuilder() is faster and easier on the system.
             */  
            var msg = new DialogMessage("RPG PLUGIN", header, message);
            ModCommunication.SendMessageTo(msg, player.SteamUserId);
        }
    }
    
    /// <summary>
    /// Manages all role classes and their configs.
    /// </summary>
    public static class RoleAgent
    {
        public static void allConfigs()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types = asm.GetTypes().Where(t => t.IsSubclassOf(typeof(configBase)));
            foreach (Type t in types)
            {
                if (t.Name == "SampleConfig") continue;
                configBase instance = (configBase)Activator.CreateInstance(t);
                instance.init();
                instance.LoadConfig();
                Roles.classConfigs.Add(t.Name, instance);
            }
        }
        
        public static void allClasses()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types = asm.GetTypes().Where(t => t.IsSubclassOf(typeof(ClassesBase)));
            foreach (Type t in types)
            {
                if (t.Name == "SampleClass") continue;
                ClassesBase instance = (ClassesBase)Activator.CreateInstance(t);
                instance.init();
                Roles.roles.Add(t.Name,instance);
            }
        }

        public static void OnLoaded()
        {
            foreach (KeyValuePair<string, ClassesBase> role in Roles.roles)   
                role.Value.Loaded();
        }

        public static void Unload()
        {
            foreach (KeyValuePair<string, ClassesBase> role in Roles.roles)   
                role.Value.Unloading();
        }
    }
}