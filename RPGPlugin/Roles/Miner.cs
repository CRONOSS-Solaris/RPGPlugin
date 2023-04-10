using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGPlugin
{
    public class Miner
    {
        private readonly int[] _expRequiredForLevels;
         
        public int MaxLevel => 10;
        public int[] ExpRequiredForLevels => _expRequiredForLevels;
        public int Level { get; set; }
        public int Exp { get; set; }

        public Miner()
        {
            _expRequiredForLevels = new int[MaxLevel];
            _expRequiredForLevels = new int[] { 0, 100, 300, 400, 500, 600, 700, 800, 900, 1000 };

        }

        private Dictionary<string, int> _oreExpValues = new Dictionary<string, int>()
    {
        { "Iron", 10 },
        { "Nickel", 10 },
        { "Cobalt", 10 },
        { "Magnesium", 10 },
        { "Silicon", 10 },
        { "Silver", 10 },
        { "Gold", 10 },
        { "Platinum", 10 },
        // dodaj kolejne surowce tutaj, np. { "Uranium", 20 },
    };

        public void AddExp(int amount, string typeId, string subtypeId)
        {
            if (!_oreExpValues.ContainsKey(subtypeId))
                return;

            if (typeId == "MyObjectBuilder_Ore")
            {
                amount *= _oreExpValues[subtypeId];
            }

            Exp += amount;
            int expRequiredForNextLevel = GetExpRequiredForNextLevel();

            if (Exp >= expRequiredForNextLevel && Level < MaxLevel)
            {
                Level++;
                Exp -= expRequiredForNextLevel;
                expRequiredForNextLevel = GetExpRequiredForNextLevel();
            }
        }

        private int GetExpRequiredForNextLevel()
        {
            if (Level >= MaxLevel)
            {
                return 0;
            }
            return ExpRequiredForLevels[Level];
        }
    }

}