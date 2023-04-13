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
            ExpPerOre = double.Parse(ExpPerOreTextBox.Text);
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
