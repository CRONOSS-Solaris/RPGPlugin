using System.Collections.Generic;
using System.Linq;
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

        private void AddNewOreButton_Click(object sender, RoutedEventArgs e)
        {
            var addOreWindow = new AddOreWindow();

            if (addOreWindow.ShowDialog() == true)
            {
                var newOres = addOreWindow.Ores;

                foreach (var newOre in newOres)
                {
                    if (!Plugin.MinerConfig.ExpRatio.ContainsKey(newOre.Key))
                    {
                        Plugin.MinerConfig.ExpRatio.Add(newOre.Key, newOre.Value);
                    }
                    else
                    {
                        MessageBox.Show($"Ore with the name '{newOre.Key}' already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                MinerConfig.SaveMinerConfig(Plugin.MinerConfig);
                ExpRatioDataGrid.Items.Refresh();
            }
        }


        private void EditOreButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExpRatioDataGrid.SelectedItem != null)
            {
                var selectedOre = (KeyValuePair<string, double>)ExpRatioDataGrid.SelectedItem;
                var editOreWindow = new EditOreWindow
                {
                    Title = "Edit ore"
                };
                editOreWindow.SetOreName(selectedOre.Key);
                editOreWindow.SetExpPerOre(selectedOre.Value);


                if (editOreWindow.ShowDialog() == true)
                {
                    string editedOreName = editOreWindow.OreName;
                    double editedOreExp = editOreWindow.ExpPerOre;

                    if (selectedOre.Key != editedOreName)
                    {
                        if (!Plugin.MinerConfig.ExpRatio.ContainsKey(editedOreName))
                        {
                            Plugin.MinerConfig.ExpRatio.Remove(selectedOre.Key);
                        }
                        else
                        {
                            MessageBox.Show("Ore with the provided name already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    Plugin.MinerConfig.ExpRatio[editedOreName] = editedOreExp;
                    MinerConfig.SaveMinerConfig(Plugin.MinerConfig);
                    ExpRatioDataGrid.Items.Refresh();
                }
            }
            else
            {
                MessageBox.Show("Please select an ore to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteOreButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExpRatioDataGrid.SelectedItems.Count > 0)
            {
                MessageBoxResult result;
                if (ExpRatioDataGrid.SelectedItems.Count == 1)
                {
                    var selectedOre = (KeyValuePair<string, double>)ExpRatioDataGrid.SelectedItem;
                    result = MessageBox.Show($"Are you sure you want to delete the ore '{selectedOre.Key}'?", "Delete Ore", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        Plugin.MinerConfig.ExpRatio.Remove(selectedOre.Key);
                        MinerConfig.SaveMinerConfig(Plugin.MinerConfig);
                        ExpRatioDataGrid.Items.Refresh();
                    }
                }
                else
                {
                    result = MessageBox.Show($"Are you sure you want to delete the selected {ExpRatioDataGrid.SelectedItems.Count} ores?", "Delete Ores", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (var selectedOre in ExpRatioDataGrid.SelectedItems.Cast<KeyValuePair<string, double>>().ToList())
                        {
                            Plugin.MinerConfig.ExpRatio.Remove(selectedOre.Key);
                        }
                        MinerConfig.SaveMinerConfig(Plugin.MinerConfig);
                        ExpRatioDataGrid.Items.Refresh();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select at least one ore to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
