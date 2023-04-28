using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Newtonsoft.Json;
using RPGPlugin.Utils;
using RPGPlugin.View;

namespace RPGPlugin
{
    public sealed class HunterConfig : configBase
    {
        // Definition of the ExpRatio property, which stores experience point values for individual minerals
        /// <inheritdoc />
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; } =
            new ObservableCollection<KeyValuePair<string, double>>();
        
        //test skill point system
        public override ObservableCollection<KeyValuePair<int, int>> SkillPoints { get; set; } =
            new ObservableCollection<KeyValuePair<int, int>>();

        public override void init()
        {
            // Initialize class with default settings, will be replaced if a config file is loaded.
            ExpRatio.Add(new KeyValuePair<string, double>("Wolf",        0.0013 ));
            ExpRatio.Add(new KeyValuePair<string, double>("Spider",      0.0013 ));
            ExpRatio.Add(new KeyValuePair<string, double>("SmallBlock",       2 ));
            ExpRatio.Add(new KeyValuePair<string, double>("LargeBlock",      10 ));

            StaticHelperFunctions.StaThreadWrapper(() =>
            {
                
                    Roles.Log.Warn($"HunterSTA Thread => {Thread.CurrentThread.ManagedThreadId}");
                    UserControl classView = new Hunter();
                    Roles.Instance.classViews.Add("Hunter", classView);
                
            });
            
        }

        public override void RegisterClass()
        {
            // Register Class Info
            SerializableTuple<string, string> RoleToRegister = new SerializableTuple<string, string>{Item1 = "Hunter", Item2 = "Specialized in attacking NPC ships and creatures."};
            if (Roles.Instance.Config.RegisteredRoles.Any(role => role.Item1.Equals(RoleToRegister.Item1, StringComparison.OrdinalIgnoreCase))) return ;
            Roles.Instance.Config.RegisteredRoles.Add(RoleToRegister);
            Roles.Log.Warn($"Registered New Class: {RoleToRegister.Item1}");
        }

        public override void LoadConfig()
        {
            string data = GetConfig().Result;
            if (data == null) return;
            HunterConfig classConfig = JsonConvert.DeserializeObject<HunterConfig>(data);
            ExpRatio = classConfig.ExpRatio;
        }

        public override async Task SaveConfig()
        {
            string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            await SaveConfig(jsonData);
        }
        
    }
}