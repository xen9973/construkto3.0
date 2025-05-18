using System.Windows;

namespace construkto3._0.Views
{
    /// <summary>
    /// Логика взаимодействия для AddCounterpartyView.xaml
    /// </summary>
    public partial class AddCounterpartyView : Window
    {
        public AddCounterpartyView()
        {
            InitializeComponent();
            SaveButton.Click += SaveButton_Click;

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
