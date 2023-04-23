using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RPGPlugin.PointManagementSystem;

namespace RPGPlugin
{
    public class SampleClass : ClassesBase
    {
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; }

        public override ObservableCollection<KeyValuePair<int, int>> SkillPoints { get; set; }
        
        // Change the rate the queue process runs.
        protected override double _queueFrequency { get; set; }

        protected override Task ProcessXpCollectedAsync()
        {
            // Calculate how muc XP is earned by the actions you've setup.
            // See MinerClass for example.
            return Task.CompletedTask;
        }

        public override int ExpToLevelUp(ulong steamID)
        {
            // Do maths to determine how much XP required to reach next level.
            // Return the required XP as a double.
            return 0;
        }

        protected override Task AddClassExp(ulong steamID, double exp)
        {
            // Look at MinerClass or WarriorClass for an example.
            return Task.CompletedTask;
        }

        public override void Loaded()
        {
            // Any calls to game events or other in game related values should start from here.
        }

        public override void Unloading()
        {
            // Release, unhook, etc everything related to the in game values.
        }

        public override void init()
        {
            // Initialize your config and all required base configs with the following:
            // YourConfigClass config = (YourConfigClass)classConfigs["YourConfigClass"]
            // if (config != null)
            //     ExpRatio = config.ExpRatio;
            base.init();
        }
    }
}