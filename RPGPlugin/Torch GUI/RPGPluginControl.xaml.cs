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

        public void Donate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/donate/?hosted_button_id=HCV9695KQDMFN");
        }

    }
}
