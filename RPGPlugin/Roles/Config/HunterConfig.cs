using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RPGPlugin
{
    public sealed class HunerConfig : configBase
    {
        // Definition of the ExpRatio property, which stores experience point values for individual minerals
        /// <inheritdoc />
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; } =
            new ObservableCollection<KeyValuePair<string, double>>();

        public override void init()
        {
            // Initialize class with default settings, will be replaced if a config file is loaded.
            ExpRatio.Add(new KeyValuePair<string, double>("Wolf",        0.0013 ));
            ExpRatio.Add(new KeyValuePair<string, double>("Spider",      0.0013 ));
            ExpRatio.Add(new KeyValuePair<string, double>("SmallBlock",       2 ));
            ExpRatio.Add(new KeyValuePair<string, double>("LargeBlock",      10 ));
        }

        public override void LoadConfig()
        {
            string data = GetConfig().Result;
            if (data == null) return;
            HunerConfig classConfig = JsonConvert.DeserializeObject<HunerConfig>(data);
            ExpRatio = classConfig.ExpRatio;
        }

        public override async Task SaveConfig()
        {
            string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            await SaveConfig(jsonData);
        }
        
    }
}