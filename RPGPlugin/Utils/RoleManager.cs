using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RPGPlugin.PointManagementSystem;

namespace RPGPlugin.Utils
{
    
    /// <summary>
    /// Manages all role classes and their configs.
    /// </summary>
    public static class RoleAgent
    {
        public static void LoadAllConfigs()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types = asm.GetTypes().Where(t => t.IsSubclassOf(typeof(configBase)));
            foreach (Type t in types)
            {
                if (t.Name == "SampleConfig") continue;
                configBase instance = (configBase)Activator.CreateInstance(t);
                instance.init();
                instance.LoadConfig();
                instance.RegisterClass();
                Roles.classConfigs.Add(t.Name, instance);
            }
        }
        
        public static void LoadAllClasses()
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