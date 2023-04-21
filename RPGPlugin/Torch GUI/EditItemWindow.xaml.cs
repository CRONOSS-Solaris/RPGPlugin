using System.Globalization;
using System.Windows;

namespace RPGPlugin
{

    public partial class EditItemWindow : Window
    {
        public ItemType ItemType { get; set; }
        public string ItemName { get; set; }
        public double ExpPerItem { get; set; }

        public EditItemWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ItemName = ItemNameTextBox.Text;

            if (double.TryParse(ExpPerItemTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double expPerItem))
            {
                ExpPerItem = expPerItem;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show($"Exp per item must be a number between 0.0 and .{double.MaxValue.ToString(CultureInfo.InvariantCulture)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetItemName(string itemName)
        {
            ItemNameTextBox.Text = itemName;
        }

        public void SetExpPerItem(double expPerItem)
        {
            ExpPerItemTextBox.Text = expPerItem.ToString(CultureInfo.InvariantCulture);
        }
    }
}