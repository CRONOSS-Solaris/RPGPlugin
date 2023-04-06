using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGPlugin
{
    public interface IRole
    {
        int MaxLevel { get; }
        int[] ExpRequiredForLevels { get; }
         
        int Level { get; set; }
        int Exp { get; set; }

        void AddExp(int amount, string typeId, string subtypeId);
    }
}
