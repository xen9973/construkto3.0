using System.Windows;
using System.Windows.Input;
using construkto3._0.Models;
using construkto3._0.Services;


namespace construkto3._0.ViewModels
{
    public class AddCounterpartyViewModel : ViewModelBase
    {
        public Counterparty NewCounterparty { get; set; } = new Counterparty();

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddCounterpartyViewModel()
        {
            SaveCommand = new RelayCommand(OnSave);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void OnSave(object parameter)
        {
            // Проверки на пустоту
            if (string.IsNullOrWhiteSpace(NewCounterparty.Name))
            {
                MessageBox.Show("Введите наименование контрагента.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DatabaseService.AddCounterparty(NewCounterparty);
                MessageBox.Show("Контрагент успешно сохранён.", "Готово",
                         MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении контрагента: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancel(object parameter)
        {
            if (parameter is Window wnd)
                wnd.Close();
        }
    }
}
