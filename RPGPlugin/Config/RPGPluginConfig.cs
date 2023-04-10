﻿using System.Collections.Generic;
using Torch;

namespace RPGPlugin
{
    public class RPGPluginConfig : ViewModel
    {

        private List<string> _RolesList = new List<string> { "Miner", "Hunter", "Warrior" };
        private string _SelectedRole;

        public List<string> RolesList { get => _RolesList; set => SetValue(ref _RolesList, value); }
        public string SelectedRole { get => _SelectedRole; set => SetValue(ref _SelectedRole, value); }

        private ushort _expMiningHandlerId = 54546;

        public ushort ExpMiningHandlerId
        {
            get => _expMiningHandlerId;
            set => SetValue(ref _expMiningHandlerId, value);
        }

        public void SetRole(string roleName)
        {
            if (RolesList.Contains(roleName))
            {
                SelectedRole = roleName;
            }
        }

    }
}