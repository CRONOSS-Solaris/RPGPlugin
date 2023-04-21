using System.Collections.Generic;
using System.Windows;

namespace RPGPlugin
{
    public enum ItemType
    {
        Ore,
        WarriorDefinition
    }

    public partial class AddItemWindow : Window
    {
        public ItemType ItemType { get; set; }
        public string ItemName { get; set; }
        public double ExpPerItem { get; set; }
        public Dictionary<string, double> Items { get; private set; } = new Dictionary<string, double>();

        public AddItemWindow()
        {
            InitializeComponent();
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            string itemName = ItemNameTextBox.Text;

            if (double.TryParse(ExpPerItemTextBox.Text, out double expPerItem))
            {
                if (!Items.ContainsKey(itemName))
                {
                    Items.Add(itemName, expPerItem);
                    ItemDataGrid.Items.Add(new { ItemName = itemName, ExpPerItem = expPerItem });
                    ItemNameTextBox.Clear();
                    ExpPerItemTextBox.Clear();
                }
                else
                {
                    MessageBox.Show($"{ItemType} with the provided name already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Exp per item must be a number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public void SetItemName(string itemName)
        {
            ItemNameTextBox.Text = itemName;
        }

        public void SetExpPerItem(double expPerItem)
        {
            ExpPerItemTextBox.Text = expPerItem.ToString();
        }
    }
}