using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace RPGPlugin
{
    public class WarriorConfig
    {
        public ObservableCollection<KeyValuePair<string,double>> ExpRatio { get; set; }
        private static readonly object _lock = new object();
        private static TimeSpan _lockTimeOut = TimeSpan.FromMilliseconds(5000);
        private const string storagePath = "Instance/RPGPlugin/";
        private static string configFilePath = Path.Combine(storagePath, "WarriorConfig.json");
        
        public double ExpPerKill { get; set; }
        public double ExpPerDestroyedBlock { get; set; }

        public WarriorConfig() { }

        private static WarriorConfig defaultConfig = new WarriorConfig
        {
            ExpRatio = new ObservableCollection<KeyValuePair<string,double>>
            {
                new KeyValuePair<string, double>(  "Player",      100  ),
                new KeyValuePair<string, double>(  "SmallBlock",  2    ),
                new KeyValuePair<string, double>(  "LargeBlock",  10   )   
            }
        };

        public void Init()
        {
            ExpPerKill = 100.0;
            ExpPerDestroyedBlock = 10.0;
        }
        
        public void Start()
        {
            
        }
    }
}
