using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace RPGPlugin
{
    public partial class RolesControl : UserControl
    {
        private Roles Plugin { get; }
        public ObservableCollection<KeyValuePair<string, double>> ExpRatio
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
            ExpRatioDataGrid.DataContext = Roles.Instance.minerConfig;
            ExpRatioDataGrid.ItemsSource = Roles.Instance.minerConfig.ExpRatio;
            BroadcastLevelUpToggleButton.IsChecked = Plugin.Config.BroadcastLevelUp;
        }

        public void Donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.paypal.com/donate/?hosted_button_id=HCV9695KQDMFN");
        }

        private async void AddNewOreButton_Click(object sender, RoutedEventArgs e)
        {
            var addOreWindow = new AddOreWindow();

            if (addOreWindow.ShowDialog() == true)
            {
                var newOres = addOreWindow.Ores;

                foreach (var newOre in newOres)
                {
                    bool found = false;
                    for (int index = Plugin.minerConfig.ExpRatio.Count - 1; index >= 0; index--)
                    {
                        var kvp = Plugin.minerConfig.ExpRatio[index];
                        if (kvp.Key != newOre.Key) continue;
                        MessageBox.Show($"Ore with the name '{newOre.Key}' already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        found = true;
                        break;
                    }

                    if (!found)
                        Plugin.minerConfig.ExpRatio.Add(new KeyValuePair<string, double>(newOre.Key, newOre.Value));
                }
                await Roles.Instance.minerConfig.SaveMinerConfig();
            }
        }

        private async void EditOreButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExpRatioDataGrid.SelectedItem != null)
            {
                var selectedOre = (KeyValuePair<string, double>)ExpRatioDataGrid.SelectedItem;
                var editOreWindow = new EditOreWindow { Title = "RPGPLUGIN - EDIT ORE" };
                
                editOreWindow.SetOreName(selectedOre.Key);
                editOreWindow.SetExpPerOre(selectedOre.Value);

                if (editOreWindow.ShowDialog() == true)
                {
                    string editedOreName = editOreWindow.OreName;
                    double editedOreExp = editOreWindow.ExpPerOre;

                    if (string.IsNullOrWhiteSpace(editedOreName))
                    {
                        MessageBox.Show("Enter a valid Ore name!!!", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    
                    Plugin.minerConfig.ExpRatio.Remove(selectedOre);
                    Plugin.minerConfig.ExpRatio.Add(new KeyValuePair<string, double>(editedOreName, editedOreExp));

                    await Roles.Instance.minerConfig.SaveMinerConfig();  // No longer blocking UI thread on IO operation.
                }
            }
            else
            {
                MessageBox.Show("Please select an ore to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteOreButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExpRatioDataGrid.SelectedItem != null)
            {
                MessageBoxResult result;
                
                var selectedOre = (KeyValuePair<string, double>)ExpRatioDataGrid.SelectedItem;
                result = MessageBox.Show($"Are you sure you want to delete the ore '{selectedOre.Key}'?", "Delete Ore", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;
                
                Plugin.minerConfig.ExpRatio.Remove(selectedOre);
                await Roles.Instance.minerConfig.SaveMinerConfig();
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

        private void BroadcastLevelUpToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Config.BroadcastLevelUp = true;
        }

        private void BroadcastLevelUpToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Config.BroadcastLevelUp = false;
        }

    }
}
