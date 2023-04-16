using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Timers;
using RPGPlugin.Utils;
using Timer = System.Timers.Timer;

namespace RPGPlugin.PointManagementSystem
{
    public sealed class MinerClass
    {
        // Miner Reward Rates
        public ObservableCollection<KeyValuePair<string, double>> ExpRatio = new ObservableCollection<KeyValuePair<string, double>>();
        public Timer _QueueTimer = new Timer();
        public ConcurrentQueue<CollectedOre> _ProcessQueue = new ConcurrentQueue<CollectedOre>();
        private Dictionary<string, double> oreTable = new Dictionary<string, double>();
            
        private static bool _queueInProcess;

        public MinerClass()
        {
            _QueueTimer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
            _QueueTimer.Elapsed += ProcessOresCollected;
            ExpRatio.CollectionChanged += UpdateLookupTable;
        }

        private void UpdateLookupTable(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var kvp in ExpRatio)
            {
                oreTable.Add(kvp.Key, kvp.Value);
            }
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
            // Create look-up table
            
            
            if (_queueInProcess) return;
            
            _queueInProcess = true;
            while (_ProcessQueue.Count > 0)
            {
                if (!_ProcessQueue.TryDequeue(out CollectedOre queueData)) continue;
                
                // If player data no loaded yet, dont crash
                if (!Roles.PlayerManagers.ContainsKey(queueData.ownerID)) continue;
                
                // If not a miner, no points given.
                if (Roles.PlayerManagers[queueData.ownerID].GetRole() != PlayerManager.FromRoles.Miner) return;
            
                await Roles.PlayerManagers[queueData.ownerID].AddMinerExp(oreTable[queueData.subType] * queueData.amount);
            }

            _queueInProcess = false;
        }
    }
}