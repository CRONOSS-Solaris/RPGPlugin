using System.Collections.Generic;
using Torch;
using VRage.Collections;

namespace RPGPlugin
{
    public class RPGPluginConfig : ViewModel
    {

        private List<string> _RolesList = new List<string> { "Miner", "Hunter", "Warrior" };
        private ushort _expMiningHandlerId = 54546;
        private ObservableCollection<string> _minerCustomSubTypes = new ObservableCollection<string>();

        public ushort ExpMiningHandlerId { get => _expMiningHandlerId; set => SetValue(ref _expMiningHandlerId, value); }

        public ObservableCollection<string> MinerCustomSubTypes { get => _minerCustomSubTypes; set => SetValue(ref _minerCustomSubTypes, value); }
    }
}
