using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RPGPlugin
{
    public sealed class MinerConfig : configBase
    {
        // Definition of the ExpRatio property, which stores experience point values for individual minerals

        /// <inheritdoc />
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; } =
            new ObservableCollection<KeyValuePair<string, double>>();

        public override void init()
        {
            base.init();
            // Set defaults, will be used if no config file has been created or becomes corrupt.
            ExpRatio.Add(new KeyValuePair<string, double>( "Stone",     0.0013 ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Silicon",   0.12   ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Iron",      0.13   ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Nickel",    0.13   ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Cobalt",    0.18   ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Magnesium", 0.24   ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Silver",    0.15   ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Gold",      0.25   ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Platinum",  0.28   ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Uranium",   0.30   ));
            ExpRatio.Add(new KeyValuePair<string, double>( "Ice",       0.135  ));
        }
        
        public override void LoadConfig()
        {
            string data = GetConfig().Result;
            if (data == null) return;
            
            MinerConfig classConfig = JsonConvert.DeserializeObject<MinerConfig>(data);
            ExpRatio = classConfig.ExpRatio;
        }

        public override async Task SaveConfig()
        {
            string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
            await SaveConfig(jsonData);
        }
    }
}







