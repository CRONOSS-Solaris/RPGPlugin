using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace RPGPlugin.View
{
    public partial class Hunter : UserControl
    {
        public Hunter()
        {
            InitializeComponent();
            DataContext = this;
            HunterDataGrid.DataContext = (HunterConfig)Roles.classConfigs["HunterConfig"];
            HunterDataGrid.ItemsSource = Roles.classConfigs["HunterConfig"].ExpRatio;
        }
        
        // Add New Hunter Definition Button
        private async void AddNewHunterDefinitionButton_Click(object sender, RoutedEventArgs e)
        {
            var addItemWindow = new AddItemWindow { ItemType = ItemType.HunterDefinition, Title = "RPGPLUGIN - ADD NEW HUNTER DEFINITION" };

            if (addItemWindow.ShowDialog() == true)
            {
                var newItems = addItemWindow.Items;

                foreach (var newItem in newItems)
                {
                    bool found = false;
                    for (int index = Roles.classConfigs["HunterConfig"].ExpRatio.Count - 1; index >= 0; index--)
                    {
                        var kvp = Roles.classConfigs["HunterConfig"].ExpRatio[index];
                        if (kvp.Key != newItem.Key) continue;
                        MessageBox.Show($"Hunter definition with the name '{newItem.Key}' already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        found = true;
                        break;
                    }

                    if (!found)
                        Roles.classConfigs["HunterConfig"].ExpRatio.Add(new KeyValuePair<string, double>(newItem.Key, newItem.Value));
                }
                await Roles.classConfigs["HunterConfig"].SaveConfig();
            }
        }
        
        //Edit Hunter Definition Button
        private async void EditHunterDefinitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (HunterDataGrid.SelectedItem != null)
            {
                var selectedItem = (KeyValuePair<string, double>)HunterDataGrid.SelectedItem;
                var editItemWindow = new EditItemWindow { ItemType = ItemType.HunterDefinition, Title = "RPGPLUGIN - EDIT HUNTER DEFINITION" };

                editItemWindow.SetItemName(selectedItem.Key);
                editItemWindow.SetExpPerItem(selectedItem.Value);

                if (editItemWindow.ShowDialog() == true)
                {
                    string editedItemName = editItemWindow.ItemName;
                    double editedItemExp = editItemWindow.ExpPerItem;

                    if (string.IsNullOrWhiteSpace(editedItemName))
                    {
                        MessageBox.Show("Enter a valid Hunter Definition name!!!", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Roles.classConfigs["HunterConfig"].ExpRatio.Remove(selectedItem);
                    Roles.classConfigs["HunterConfig"].ExpRatio.Add(new KeyValuePair<string, double>(editedItemName, editedItemExp));

                    await Roles.classConfigs["HunterConfig"].SaveConfig();
                }
            }
            else
            {
                MessageBox.Show("Please select a Hunter definition to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Delete Hunter Definition Button
        private async void DeleteHunterDefinitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (HunterDataGrid.SelectedItem != null)
            {
                MessageBoxResult result;

                var selectedOre = (KeyValuePair<string, double>)HunterDataGrid.SelectedItem;
                result = MessageBox.Show($"Are you sure you want to delete the HunterDefinition '{selectedOre.Key}'?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                Roles.classConfigs["HunterConfig"].ExpRatio.Remove(selectedOre);
                await Roles.classConfigs["HunterConfig"].SaveConfig();
            }
            else
            {
                MessageBox.Show("Please select at least one HunterDefinition to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}