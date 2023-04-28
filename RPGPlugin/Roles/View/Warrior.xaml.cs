using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RPGPlugin.View
{
    public partial class Warrior : UserControl
    {
        public Warrior()
        {
            InitializeComponent();
            WarriorDataGrid.DataContext = (WarriorConfig)Roles.classConfigs["WarriorConfig"];
            WarriorDataGrid.ItemsSource = Roles.classConfigs["WarriorConfig"].ExpRatio;
        }
        
        // Add New Warrior Definition Button
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
    
    //Edit Warrior Definition Button
        private async void EditWarriorDefinitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (WarriorDataGrid.SelectedItem != null)
            {
                var selectedItem = (KeyValuePair<string, double>)WarriorDataGrid.SelectedItem;
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
        
        // Delete Warrior Definition Button
        private async void DeleteWarriorDefinitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (WarriorDataGrid.SelectedItem != null)
            {
                MessageBoxResult result;

                var selectedOre = (KeyValuePair<string, double>)WarriorDataGrid.SelectedItem;
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
    }
}