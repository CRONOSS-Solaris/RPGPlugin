using System.Windows;
using System.Windows.Controls;

namespace RPGPlugin
{
    public partial class RolesControl : UserControl
    {

        private Roles Plugin { get; }

        private RolesControl()
        {
            InitializeComponent();
        }

        public RolesControl(Roles plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
        }

    }
}
