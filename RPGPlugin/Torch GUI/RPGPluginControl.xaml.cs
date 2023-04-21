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
            get => Roles.classConfigs["MinerConfig"].ExpRatio;
            set => Roles.classConfigs["MinerConfig"].ExpRatio = value;
        }

        private RolesControl()
        {
            InitializeComponent();
        }

        public RolesControl(Roles plugin) : this()
        {
            Plugin = plugin;
            DataContext = this;
            ExpRatioDataGrid.DataContext = (MinerConfig)Roles.classConfigs["MinerConfig"];
            ExpRatioDataGrid.ItemsSource = Roles.classConfigs["MinerConfig"].ExpRatio;
            WarriorDataGrid.DataContext = (WarriorConfig)Roles.classConfigs["WarriorConfig"];
            WarriorDataGrid.ItemsSource = Roles.classConfigs["WarriorConfig"].ExpRatio;
        }

        public void Donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.paypal.com/donate/?hosted_button_id=HCV9695KQDMFN");
        }

        private async void AddNewOreButton_Click(object sender, RoutedEventArgs e)
        {
            var addItemWindow = new AddItemWindow { ItemType = ItemType.Ore, Title = "RPGPLUGIN - ADD NEW ORE" };

            if (addItemWindow.ShowDialog() == true)
            {
                var newItems = addItemWindow.Items;

                foreach (var newItem in newItems)
                {
                    bool found = false;
                    for (int index = Roles.classConfigs["MinerConfig"].ExpRatio.Count - 1; index >= 0; index--)
                    {
                        var kvp = Roles.classConfigs["MinerConfig"].ExpRatio[index];
                        if (kvp.Key != newItem.Key) continue;
                        MessageBox.Show($"Ore with the name '{newItem.Key}' already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        found = true;
                        break;
                    }

                    if (!found)
                        Roles.classConfigs["MinerConfig"].ExpRatio.Add(new KeyValuePair<string, double>(newItem.Key, newItem.Value));
                }
                await Roles.classConfigs["MinerConfig"].SaveConfig();
            }
        }

        private async void AddNewWarriorDefinitionButton_Click(object sender, RoutedEventArgs e)
        {
            var addItemWindow = new AddItemWindow { ItemType = ItemType.WarriorDefinition, Title = "RPGPLUGIN - ADD NEW WARRIOR DEFINITION" };

            if (addItemWindow.ShowDialog() == true)
            {
                var newItems = addItemWindow.Items;

                foreach (var newItem in newItems)
                {
                    bool found = false;
                    for (int index = Roles.classConfigs["WarriorConfig"].ExpRatio.Count - 1; index >= 0; index--)
                    {
                        var kvp = Roles.classConfigs["WarriorConfig"].ExpRatio[index];
                        if (kvp.Key != newItem.Key) continue;
                        MessageBox.Show($"Warrior definition with the name '{newItem.Key}' already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        found = true;
                        break;
                    }

                    if (!found)
                        Roles.classConfigs["WarriorConfig"].ExpRatio.Add(new KeyValuePair<string, double>(newItem.Key, newItem.Value));
                }
                await Roles.classConfigs["WarriorConfig"].SaveConfig();
            }
        }


        private async void EditOreButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExpRatioDataGrid.SelectedItem != null)
            {
                var selectedItem = (KeyValuePair<string, double>)ExpRatioDataGrid.SelectedItem;
                var editItemWindow = new EditItemWindow { ItemType = ItemType.Ore, Title = "RPGPLUGIN - EDIT ORE" };

                editItemWindow.SetItemName(selectedItem.Key);
                editItemWindow.SetExpPerItem(selectedItem.Value);

                if (editItemWindow.ShowDialog() == true)
                {
                    string editedItemName = editItemWindow.ItemName;
                    double editedItemExp = editItemWindow.ExpPerItem;

                    if (string.IsNullOrWhiteSpace(editedItemName))
                    {
                        MessageBox.Show("Enter a valid Ore name!!!", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Roles.classConfigs["MinerConfig"].ExpRatio.Remove(selectedItem);
                    Roles.classConfigs["MinerConfig"].ExpRatio.Add(new KeyValuePair<string, double>(editedItemName, editedItemExp));

                    await Roles.classConfigs["MinerConfig"].SaveConfig();  // No longer blocking UI thread on IO operation.
                }
            }
            else
            {
                MessageBox.Show("Please select an ore to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditWarriorDefinitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExpRatioDataGrid.SelectedItem != null)
            {
                var selectedItem = (KeyValuePair<string, double>)ExpRatioDataGrid.SelectedItem;
                var editItemWindow = new EditItemWindow { ItemType = ItemType.WarriorDefinition, Title = "RPGPLUGIN - EDIT WARRIOR DEFINITION" };

                editItemWindow.SetItemName(selectedItem.Key);
                editItemWindow.SetExpPerItem(selectedItem.Value);

                if (editItemWindow.ShowDialog() == true)
                {
                    string editedItemName = editItemWindow.ItemName;
                    double editedItemExp = editItemWindow.ExpPerItem;

                    if (string.IsNullOrWhiteSpace(editedItemName))
                    {
                        MessageBox.Show("Enter a valid Warrior Definition name!!!", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Roles.classConfigs["WarriorConfig"].ExpRatio.Remove(selectedItem);
                    Roles.classConfigs["WarriorConfig"].ExpRatio.Add(new KeyValuePair<string, double>(editedItemName, editedItemExp));

                    await Roles.classConfigs["WarriorConfig"].SaveConfig();
                }
            }
            else
            {
                MessageBox.Show("Please select a warrior definition to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                
                Roles.classConfigs["MinerConfig"].ExpRatio.Remove(selectedOre);
                await Roles.classConfigs["MinerConfig"].SaveConfig();
            }
            else
            {
                MessageBox.Show("Please select at least one ore to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteWarriorDefinitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExpRatioDataGrid.SelectedItem != null)
            {
                MessageBoxResult result;

                var selectedOre = (KeyValuePair<string, double>)ExpRatioDataGrid.SelectedItem;
                result = MessageBox.Show($"Are you sure you want to delete the WarriorDefinition '{selectedOre.Key}'?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                Roles.classConfigs["WarriorConfig"].ExpRatio.Remove(selectedOre);
                await Roles.classConfigs["WarriorConfig"].SaveConfig();
            }
            else
            {
                MessageBox.Show("Please select at least one WarriorDefinition to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
