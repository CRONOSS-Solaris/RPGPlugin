using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RPGPlugin
{
    public sealed class WarriorConfig : configBase
    {
        // Definition of the ExpRatio property, which stores experience point values for individual minerals
        /// <inheritdoc />
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; } =
            new ObservableCollection<KeyValuePair<string, double>>();

        public override void init()
        {
            // Initialize class with default settings, will be replaced if a config file is loaded.
            ExpRatio.Add(new KeyValuePair<string, double>("EnemyPlayer",        25  ));
            ExpRatio.Add(new KeyValuePair<string, double>("EnemySmallBlock",    0.2 ));
            ExpRatio.Add(new KeyValuePair<string, double>("EnemyLargeBlock",    0.8 ));
            
            // 0 point values can be omitted.  If its not in the list, its the same as 0
            // ExpRatio.Add(new KeyValuePair<string, double>("FriendlySmallBlock", 0   ));
            // ExpRatio.Add(new KeyValuePair<string, double>("FriendlyLargeBlock", 0   ));
        }

        public override void LoadConfig()
        {
            string data = GetConfig().Result;
            if (data == null) return;
            WarriorConfig classConfig = JsonConvert.DeserializeObject<WarriorConfig>(data);
            ExpRatio = classConfig.ExpRatio;
        }

        public override async Task SaveConfig()
        {
            string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            await SaveConfig(jsonData);
        }
    
    }
}