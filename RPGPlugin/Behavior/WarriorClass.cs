using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RPGPlugin.PointManagementSystem
{
    public class WarriorClass : ClassesBase
    {
        public override ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; }
        protected override double _queueFrequency { get; set; }
        protected override Task ProcessXpCollectedAsync()
        {
            throw new System.NotImplementedException();
        }

        public override int ExpToLevelUp(long id)
        {
            throw new System.NotImplementedException();
        }

        protected override Task AddClassExp(long id, double exp)
        {
            throw new System.NotImplementedException();
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