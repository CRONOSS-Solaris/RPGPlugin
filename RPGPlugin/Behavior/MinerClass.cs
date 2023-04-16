using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using RPGPlugin.Utils;
using Timer = System.Timers.Timer;

namespace RPGPlugin.PointManagementSystem
{
    public sealed class MinerClass
    {
        // Miner Reward Rates
        public ConcurrentObservableDictionary<string, double> ExpRatio => Roles.Instance.minerConfig.ExpRatio;
        public Timer _QueueTimer = new Timer();
        public ConcurrentQueue<CollectedOre> _ProcessQueue = new ConcurrentQueue<CollectedOre>();
        private static bool _queueInProcess;

        public MinerClass()
        {
            _QueueTimer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
            _QueueTimer.Elapsed += ProcessOresCollected;
        }
        
        public void Start()
        {
            _QueueTimer.Start();
            ProcessOresCollected(null,null);
        }
        
        private async void ProcessOresCollected(object state, ElapsedEventArgs elapsedEventArgs)
        {
            await ProcessOresCollectedAsync();
        }

        private async Task ProcessOresCollectedAsync()
        {
            if (_queueInProcess) return;
            
            _queueInProcess = true;
            while (_ProcessQueue.Count > 0)
            {
                if (!_ProcessQueue.TryDequeue(out CollectedOre queueData)) continue;
                
                // If player data no loaded yet, dont crash
                if (!Roles.PlayerManagers.ContainsKey(queueData.ownerID)) continue;
                
                // If not a miner, no points given.
                if (Roles.PlayerManagers[queueData.ownerID].GetRole() != PlayerManager.FromRoles.Miner) return;
            
                await Roles.PlayerManagers[queueData.ownerID].AddMinerExp(ExpRatio[queueData.subType]);  
                // Tested with 0.3 and that was allot of points very fast with 13 drills!!
                // Thousands of points in 1 minute.  So will need to be very small numbers!!!
            }

            _queueInProcess = false;
        }
    }
}