using System.Windows;

namespace RPGPlugin
{
    public partial class AddOreWindow : Window
    {
        public string OreName { get; set; }
        public double ExpPerOre { get; set; }

        public AddOreWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OreName = OreNameTextBox.Text;

            if (double.TryParse(ExpPerOreTextBox.Text, out double expPerOre))
            {
                ExpPerOre = expPerOre;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Exp per ore must be a number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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