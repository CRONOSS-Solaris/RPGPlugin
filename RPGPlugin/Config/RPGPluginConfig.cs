using System.Collections.Generic;
using Torch;

namespace RPGPlugin
{
    public class RPGPluginConfig : ViewModel
    {

        private List<string> _RolesList = new List<string> { "Miner", "Hunter", "Warrior" };
        private ushort _expMiningHandlerId = 54546;
        public ushort ExpMiningHandlerId { get => _expMiningHandlerId; set => SetValue(ref _expMiningHandlerId, value); }

        

    }
}
