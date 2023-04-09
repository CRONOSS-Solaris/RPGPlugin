using System.Collections.Generic;
using Torch;

namespace RPGPlugin
{
    public class RPGPluginConfig : ViewModel
    {

        private List<string> _RolesList = new List<string> { "Miner", "Hunter", "Warrior" };
        private string _SelectedRole;

        public List<string> RolesList { get => _RolesList; set => SetValue(ref _RolesList, value); }
        public string SelectedRole { get => _SelectedRole; set => SetValue(ref _SelectedRole, value); }

        public ushort ExpMiningHandlerId { get; set; } = 54546;

        public void SetRole(string roleName)
        {
            if (RolesList.Contains(roleName))
            {
                SelectedRole = roleName;
            }
        }

    }
}
