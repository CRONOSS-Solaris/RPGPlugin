namespace RPGPlugin.PointManagementSystem
{
    public class PointManager
    {
        // Separate the classes to update, manage, etc easier.
        public readonly MinerClass MinerProtocol = new MinerClass();
        //public readonly WarriorClass WarriorProtocol = new WarriorClass();

        
        // Run methods to start each roles required inits.
        public void Start()
        {
            MinerProtocol.Start();
           // WarriorProtocol.Start();
        }



        




    }
}