using System.Collections.Generic;
using Torch;
using VRage.Collections;

namespace RPGPlugin
{
    public class RPGPluginConfig : ViewModel
    {

        private List<string> _RolesList = new List<string> { "Miner", "Hunter", "Warrior" };
        private bool _broadcastLevelUp = true;
        
        public bool BroadcastLevelUp { get => _broadcastLevelUp; set => SetValue(ref _broadcastLevelUp, value); }

    }
}
