using System.Windows;
using System.Windows.Controls;

namespace Roles
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

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
        }
    }
}
