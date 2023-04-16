using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;

namespace RPGPlugin
{
    public partial class RolesControl : UserControl
    {
        private Roles Plugin { get; }
        public ConcurrentObservableDictionary<string, double> ExpRatio
        {
            get => Plugin.minerConfig.ExpRatio;
            set => Plugin.minerConfig.ExpRatio = value;
        }
        
        private RolesControl()
        {
            InitializeComponent();
        }

        public RolesControl(Roles plugin) : this()
        {
            Plugin = plugin;
            DataContext = this;
        }

        public void Donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.paypal.com/donate/?hosted_button_id=HCV9695KQDMFN");
        }

        private async void ExpRatioDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;
            
            var mineral = ((KeyValuePair<string, double>)e.Row.Item).Key;
            var newValue = double.Parse(((TextBox)e.EditingElement).Text);
            Plugin.minerConfig.ExpRatio[mineral] = newValue;
            await Roles.Instance.minerConfig.SaveMinerConfig();
        }

        private async void AddNewOreButton_Click(object sender, RoutedEventArgs e)
        {
            var addOreWindow = new AddOreWindow();

            if (addOreWindow.ShowDialog() == true)
            {
                var newOres = addOreWindow.Ores;

                foreach (var newOre in newOres)
                {
                    if (!Plugin.minerConfig.ExpRatio.ContainsKey(newOre.Key))
                    {
                        Plugin.minerConfig.ExpRatio.TryAdd(newOre.Key, newOre.Value);
                    }
                    else
                    {
                        MessageBox.Show($"Ore with the name '{newOre.Key}' already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                await Roles.Instance.minerConfig.SaveMinerConfig();
                ExpRatioDataGrid.Items.Refresh();
            }
        }


        private async void EditOreButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExpRatioDataGrid.SelectedItem != null)
            {
                var selectedOre = (KeyValuePair<string, double>)ExpRatioDataGrid.SelectedItem;
                var editOreWindow = new EditOreWindow
                {
                    Title = "RPGPLUGIN - EDIT ORE"
                };
                editOreWindow.SetOreName(selectedOre.Key);
                editOreWindow.SetExpPerOre(selectedOre.Value);

                if (editOreWindow.ShowDialog() == true)
                {
                    string editedOreName = editOreWindow.OreName;
                    double editedOreExp = editOreWindow.ExpPerOre;

                    if (selectedOre.Key != editedOreName)
                    {
                        if (!ExpRatio.ContainsKey(editedOreName))
                        {
                            ExpRatio.Remove(selectedOre.Key);
                        }
                        else
                        {
                            MessageBox.Show("Ore with the provided name already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    ExpRatio[editedOreName] = editedOreExp;
                    await Roles.Instance.minerConfig.SaveMinerConfig();  // No longer blocking UI thread on IO operation.
                    ExpRatioDataGrid.Items.Refresh();
                }
            }
            else
            {
                MessageBox.Show("Please select an ore to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteOreButton_Click(object sender, RoutedEventArgs e)
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
                        Plugin.minerConfig.ExpRatio.Remove(selectedOre.Key);
                        await Roles.Instance.minerConfig.SaveMinerConfig();
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
                            Plugin.minerConfig.ExpRatio.Remove(selectedOre.Key);
                        }
                        await Roles.Instance.minerConfig.SaveMinerConfig();
                        ExpRatioDataGrid.Items.Refresh();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select at least one ore to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            string discordInviteLink = "https://discord.com/invite/TqbCaHu7wr";
            Process.Start(new ProcessStartInfo
            {
                FileName = discordInviteLink,
                UseShellExecute = true
            });
        }

    }
}
