using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Newtonsoft.Json;
using RPGPlugin.Utils;
using RPGPlugin.View;
using static RPGPlugin.Utils.StaticHelperFunctions;

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

        public override Task init()
        {
            // Initialize class with default settings, will be replaced if a config file is loaded.
            ExpRatio.Add(new KeyValuePair<string, double>("Wolf",        0.0013 ));
            ExpRatio.Add(new KeyValuePair<string, double>("Spider",      0.0013 ));
            ExpRatio.Add(new KeyValuePair<string, double>("SmallBlock",       2 ));
            ExpRatio.Add(new KeyValuePair<string, double>("LargeBlock",      10 ));
            
            // Init View
            
            StaThreadWrapper(() =>
            {
                UserControl view = new HunterView();
                view.Visibility = System.Windows.Visibility.Visible;
                TabItem item = new TabItem {Header = "Hunter", Content = view};
                
                Roles.Instance.ClassViews.Add(item);
            });
            
            return Task.CompletedTask;
        }

        public override Task RegisterClass()
        {
            // Register Class Info
            SerializableTuple<string, string> RoleToRegister = new SerializableTuple<string, string>{Item1 = "Hunter", Item2 = "Specialized in attacking NPC ships and creatures."};
            if (Roles.Instance.Config.RegisteredRoles.Any(role => role.Item1.Equals(RoleToRegister.Item1, StringComparison.OrdinalIgnoreCase))) return Task.CompletedTask;
            Roles.Instance.Config.RegisteredRoles.Add(RoleToRegister);
            Roles.Log.Warn($"Registered New Class: {RoleToRegister.Item1}");
            
            return Task.CompletedTask;
        }

        public override Task LoadConfig()
        {
            string data = GetConfig().Result;
            if (data == null) return Task.CompletedTask;
            HunterConfig classConfig = JsonConvert.DeserializeObject<HunterConfig>(data);
            ExpRatio = classConfig.ExpRatio;
            return Task.CompletedTask;
        }

        public override async Task SaveConfig()
        {
            string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            await SaveConfig(jsonData);
        }
        
    }
}