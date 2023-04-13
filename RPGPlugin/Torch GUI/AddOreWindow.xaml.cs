using System.Collections.Generic;
using System.Windows;

namespace RPGPlugin
{
    public partial class AddOreWindow : Window
    {
        public string OreName { get; set; }
        public double ExpPerOre { get; set; }
        public Dictionary<string, double> Ores { get; private set; } = new Dictionary<string, double>();

        public AddOreWindow()
        {
            InitializeComponent();
        }

        private void AddOreButton_Click(object sender, RoutedEventArgs e)
        {
            string oreName = OreNameTextBox.Text;

            if (double.TryParse(ExpPerOreTextBox.Text, out double expPerOre))
            {
                if (!Ores.ContainsKey(oreName))
                {
                    Ores.Add(oreName, expPerOre);
                    OreDataGrid.Items.Add(new { OreName = oreName, ExpPerOre = expPerOre });
                    OreNameTextBox.Clear();
                    ExpPerOreTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("Ore with the provided name already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Exp per ore must be a number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public void SetOreName(string oreName)
        {
            OreNameTextBox.Text = oreName;
        }

        public void SetExpPerOre(double expPerOre)
        {
            ExpPerOreTextBox.Text = expPerOre.ToString();
        }
    }
}
