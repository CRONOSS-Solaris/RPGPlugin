using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RPGPlugin.Utils;

namespace RPGPlugin.PointManagementSystem
{
    public static class PointManager
    {
        // Miner Reward Rates
        private static Dictionary<string, double> ExpRatio;
        private static Timer _QueueTimer;
        public static ConcurrentQueue<CollectedOre> _ProcessQueue = new ConcurrentQueue<CollectedOre>();
        private static bool _queueInProcess;

        public static Task Start()
        { 
            ExpRatio = MinerConfig.LoadMinerConfig().ExpRatio;
            _QueueTimer = new Timer(Callback, null, 30, 5000);
            Roles.Log.Warn("_QueueTimer will start in 30 seconds.");
            return Task.CompletedTask; // Easy way to await and not block any important threads during IO operation.
        }

        private static async void Callback(object state)
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

        //// Method to add experience for a hunter for killing another player
        //public async Task AddWarriorExpForKill(long warriorID)
        //{
        //    // Checking if the player data is available
        //    If(!Roles.PlayerManagers.ContainsKey(warriorID)) return;

        //    // Checking if the player's role is a hunter
        //    If(Roles.PlayerManagers[warriorID].GetRole() != PlayerManager.FromRoles.Warrior) return;

        //    // Retrieve the experience value per kill from the configuration
        //    double expPerKill = WarriorConfig.LoadWarriorConfig().ExpPerKill;

        //    // Adding experience for the hunter
        //    await Roles.PlayerManagers[warriorID].AddWarriorExp(expPerKill);
        //}

        //// Method that adds experience for the hunter for destroying an enemy player's structure block
        //Public async Task AddWarriorExpForDestroyedBlock(long warriorID, string blockType)
        //{
        //    // Checking if the player data is available
        //    If(!Roles.PlayerManagers.ContainsKey(warriorID)) return;

        //    // Checking if the player's role is a hunter
        //    If(Roles.PlayerManagers[warriorID].GetRole() != PlayerManager.FromRoles.Warrior) return;

        //    // Retrieving the experience value for destroying a block from the configuration
        //    double expPerDestroyedBlock = WarriorConfig.LoadWarriorConfig().ExpRatio[blockType];

        //    // Adding experience for the hunter
        //    await Roles.PlayerManagers[warriorID].AddWarriorExp(expPerDestroyedBlock);
        //}

        //Add an event or modify an existing event that is triggered when a player kills another player. 

        //Then call AddWarriorExpForKill with the player ID as an argument.

        //Add an event or modify an existing event that is triggered when a player destroys an enemy player's structure block. Then call 

        //AddWarriorExpForDestroyedBlock with the player ID and the type of block destroyed as arguments.

        //Make sure you load the hunter's configuration (WarriorConfig) similar to the miner's configuration (MinerConfig). You can do this by adding

        //WarriorConfig.LoadWarriorConfig() in the appropriate place, just as you did for MinerConfig.LoadMinerConfig().
       
        
        
        
    }
}