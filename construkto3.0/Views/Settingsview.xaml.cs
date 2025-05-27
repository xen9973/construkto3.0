using System.Windows;
using construkto3._0.ViewModels;

namespace construkto3._0.Views
{
    public partial class Settingsview : Window
    {
        private readonly SettingsViewModel _viewModel;

        public Settingsview()
        {
            InitializeComponent();
            _viewModel = (SettingsViewModel)DataContext;
        }

        private void Card_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadImage();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveData();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}