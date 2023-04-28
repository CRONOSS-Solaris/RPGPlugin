using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RPGPlugin.PointManagementSystem;

namespace RPGPlugin.Utils
{
    
    /// <summary>
    /// Manages all role classes and their configs.
    /// </summary>
    public static class RoleAgent
    {
        // Increase load performance by putting this all in one single search through the assembly.
        // instead of searching through the assembly for one type, than searching again for the next...
        // Removed LINQ cause that's slow as hell, use reverse for loop is very fast and efficient.

        private static Assembly asm = Assembly.GetExecutingAssembly();

        public static Task LoadAllRoles()
        {
            Roles.Log.Warn($"RoleManager Thread => {Thread.CurrentThread.ManagedThreadId}");
            Type[] types = asm.GetTypes();
            for (int index = types.Length - 1; index >= 0; index--)
            {
                if (types[index].IsSubclassOf(typeof(configBase)))
                {
                    if (types[index].Name == "SampleConfig") continue;
                    configBase instance = (configBase) Activator.CreateInstance(types[index]);
                    Roles.classConfigs.Add(types[index].Name, instance);
                }

                if (types[index].IsSubclassOf(typeof(ClassesBase)))
                {
                    if (types[index].Name == "SampleClass") continue;
                    ClassesBase instance = (ClassesBase) Activator.CreateInstance(types[index]);
                    Roles.roles.Add(types[index].Name, instance);
                }
            }
            
            foreach (KeyValuePair<string, configBase> config in Roles.classConfigs)
            {
                config.Value.init();
                config.Value.LoadConfig();
                config.Value.RegisterClass();
            }
            return Task.CompletedTask;
        }

        public static List<TabItem> GetRoleViews()
        {
            Type[] types = asm.GetTypes();
            List<TabItem> tabs = new List<TabItem>();
            
            foreach (KeyValuePair<string, configBase> role in Roles.classConfigs)
            {
                UserControl classView = null;
                TabItem newTab = new TabItem();
                
                for (int index = types.Length - 1; index >= 0; index--)
                {
                    if (types[index].Name != role.Value.ViewName) continue;
                    
                    classView = (UserControl) Activator.CreateInstance(types[index]);
                    newTab.Header = types[index].Name;
                    newTab.Content = classView;
                    newTab.Style= (Style) classView.FindResource("TabItemStyle");
                    tabs.Add(newTab);
                }
            }
            return tabs;
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