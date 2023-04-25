using System;
using System.Collections.Generic;
using Torch;
using Newtonsoft.Json;


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

        // ROLE, Description  -  Registered by the class itself.
        private List<Tuple<string, string>> _registeredRoles = new List<Tuple<string, string>>();
        public List<Tuple<string, string>> RegisteredRoles { get => _registeredRoles; set => SetValue(ref _registeredRoles, value); }
    }
}
