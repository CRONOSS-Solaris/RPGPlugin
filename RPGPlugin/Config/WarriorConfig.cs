//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Torch;

//namespace RPGPlugin
//{
//    public class WarriorConfig : ViewModel
//    {
//        public Dictionary<string, double> ExpRatio { get; set; }
//        public double ExpPerKill { get; set; }
//        public double ExpPerDestroyedBlock { get; set; }

//        public WarriorConfig()
//        {
//            ExpRatio = new Dictionary<string, double>
//            {
//              { "Player", 100.0 }, // Example of experience value for killing a player
//              { "SmallBlock", 2.0 }, // Example of experience value for destroying a small block
//              { "LargeBlock", 10.0 } // Example of experience value for destroying a large block
//            };

//            ExpPerKill = 100.0;
//            ExpPerDestroyedBlock = 10.0;
//        }
//    }
//}
