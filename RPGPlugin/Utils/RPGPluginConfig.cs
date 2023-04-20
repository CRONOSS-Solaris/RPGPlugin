using System.Collections.Generic;
using Torch;
using VRage.Collections;

namespace RPGPlugin
{
    public class RPGPluginConfig : ViewModel
    {
        private bool _broadcastLevelUp = true;
        public bool BroadcastLevelUp { get => _broadcastLevelUp; set => SetValue(ref _broadcastLevelUp, value); }

        // Owners can set this location to a shared network folder and point all instances to this for multi
        // multi server setups like Nexus or WormHole.  
        private string _saveLocation;
        public string SaveLocation { get => _saveLocation; set => SetValue(ref _saveLocation, value); }
    }
}
