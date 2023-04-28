using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using NLog;
using RPGPlugin.Utils;
using Timer = System.Timers.Timer;

namespace RPGPlugin
{
    public partial class RolesControl : UserControl
    {
        private Roles Plugin { get; }
        private Timer _delayLoad = new Timer(TimeSpan.FromSeconds(3).TotalMilliseconds);

        private RolesControl()
        {
            InitializeComponent();
        }

        private void DelayLoadOnElapsed(object sender, ElapsedEventArgs e)
        {
            
                DelayAsyncWorker(sender, e);
           
        }

        private Task DelayAsyncWorker(object sender, ElapsedEventArgs e)
        {
            Roles.Log.Warn($"Timer Thread => {Thread.CurrentThread.ManagedThreadId}");
            _delayLoad.Elapsed -= DelayLoadOnElapsed;
            
            foreach (KeyValuePair<string,UserControl> classView in Roles.Instance.classViews)
            {
                Roles.Instance.MainDispatcher.Invoke(() => // Were on the timer thread, method needs to run on UI thread.
                {
                    Helper(classView);
                });
                
            }

            return Task.CompletedTask;
        }

        private void Helper(KeyValuePair<string, UserControl> classView)
        {
            TabItem newView = new TabItem
            {
                Header = classView.Key,
                Content = classView.Value
            };
            
            Roles.Log.Warn("Adding TabItem to SettingsTab.");
            SettingsTab.Items.Add(newView);
        }

        public RolesControl(Roles plugin) : this()
        {
            Plugin = plugin;
            DataContext = this;
            BaseSaveLocation.DataContext = plugin.Config;
            SettingsTab.DataContext = plugin.Config;
            _delayLoad.Elapsed += DelayLoadOnElapsed;
            _delayLoad.Start(); 
            Roles.Log.Warn($"Main View Thread => {Thread.CurrentThread.ManagedThreadId}");
            
            // ** These should go to a view for each class at some point.  Each view can register in code behind. 
            // RPGPluginControl.xaml can add them in code behind.  Create a tab for each registered view and use the view for tabitem content.
            // Miner Config
            /*
             ExpRatioDataGrid.DataContext = (MinerConfig)Roles.classConfigs["MinerConfig"];
            ExpRatioDataGrid.ItemsSource = Roles.classConfigs["MinerConfig"].ExpRatio;
            // Warrior Config
            WarriorDataGrid.DataContext = (WarriorConfig)Roles.classConfigs["WarriorConfig"];
            WarriorDataGrid.ItemsSource = Roles.classConfigs["WarriorConfig"].ExpRatio;
            // Hunter Config
            HunterDataGrid.DataContext = (HunterConfig)Roles.classConfigs["HunterConfig"];
            HunterDataGrid.ItemsSource = Roles.classConfigs["HunterConfig"].ExpRatio;
            */
        }

        public void Donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.paypal.com/donate/?hosted_button_id=HCV9695KQDMFN");
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

                    await Roles.classConfigs["MinerConfig"].SaveConfig();  // No longer blocking UI thread on IO operation.
                }
            }
            else
            {
                MessageBox.Show("Please select an ore to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
