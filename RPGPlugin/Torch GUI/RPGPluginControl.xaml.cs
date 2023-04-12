using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
            DataContext = plugin;
        }

        public void Donate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/donate/?hosted_button_id=HCV9695KQDMFN");
        }

        private void ExpRatioDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var mineral = ((KeyValuePair<string, double>)e.Row.Item).Key;
                var newValue = double.Parse(((TextBox)e.EditingElement).Text);
                Plugin.MinerConfig.ExpRatio[mineral] = newValue;
                MinerConfig.SaveMinerConfig(Plugin.MinerConfig);
            }
        }

    }
}
