﻿using System.Collections.Generic;
using Torch;

namespace Roles
{
    public class RPGPluginConfig : ViewModel
    {

        private List<string> _RolesList = new List<string> { "Miner", "Hunter", "Warrior" };
        private string _SelectedRole;

        public List<string> RolesList { get => _RolesList; set => SetValue(ref _RolesList, value); }
        public string SelectedRole { get => _SelectedRole; set => SetValue(ref _SelectedRole, value); }
         
        public void SetRole(string roleName)
        {
            if (RolesList.Contains(roleName))
            {
                SelectedRole = roleName;
            }
        }

    }
}
