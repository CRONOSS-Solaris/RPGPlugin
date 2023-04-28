using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RPGPlugin.View
{
    public partial class Miner : UserControl
    {
        public Miner()
        {
            InitializeComponent();
            ExpRatioDataGrid.DataContext = (MinerConfig)Roles.classConfigs["MinerConfig"];
            ExpRatioDataGrid.ItemsSource = Roles.classConfigs["MinerConfig"].ExpRatio;
        }
        
        // Add New Ore Button | Miner
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
        
        // Edit Ore Button | Miner
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

                    await global::RPGPlugin.Roles.classConfigs["MinerConfig"].SaveConfig();  // No longer blocking UI thread on IO operation.
                }
            }
            else
            {
                MessageBox.Show("Please select an ore to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        //Delete Ore Button | Miner
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
    }
}