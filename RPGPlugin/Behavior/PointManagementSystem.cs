using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RPGPlugin.Utils;
using VRage.Collections;
using VRage.Network;

namespace RPGPlugin.PointManagementSystem
{
    public class PointManager : IDisposable
    {
        // Miner Reward Rates
        private static Dictionary<string, double> ExpRatio;
        private Timer _QueueTimer;
        public readonly MyConcurrentQueue<CollectedOre> _ProcessQueue = new MyConcurrentQueue<CollectedOre>();
        private static bool _queueInProcess;

        public Task Init()
        { 
            ExpRatio = MinerConfig.LoadMinerConfig().ExpRatio;
            _QueueTimer = new Timer(Callback, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
            return Task.CompletedTask; // Easy way to await and not block any important threads during IO operation.
        }

        private async void Callback(Object state)
        {
            // We can continue to store the data but dont process until ready.
            if (!Roles.Instance.DelayFinished) return;
            
            _queueInProcess = true;
            while (_ProcessQueue.Count > 0)
            {
                if (!_ProcessQueue.TryDequeue(out CollectedOre queueData)) continue;
                
                // If player data no loaded yet, dont crash
                if (!Roles.PlayerManagers.ContainsKey(queueData.ownerID)) continue;
                
                // If not a miner, no points given.
                if (Roles.PlayerManagers[queueData.ownerID].GetRole() != PlayerManager.FromRoles.Miner) return;
            
                // 0.5 was still crazy high lol.  // This will be removed and appropriate values used in the exp chart.
                double points = 0.03 * ExpRatio[queueData.subType];
            
                await Roles.PlayerManagers[queueData.ownerID].AddMinerExp(points);  
                // Tested with 0.3 and that was allot of points very fast with 13 drills!!
                // Thousands of points in 1 minute.  So will need to be very small numbers!!!
            }

            _queueInProcess = false;
        }
        
        // WARRIOR SECTION

        
        
        
        
        public void Dispose()
        {
            _QueueTimer?.Dispose();
        }
    }
}