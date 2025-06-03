using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using construkto3._0.Models;
using construkto3._0.Services;
using construkto3._0.Views;


namespace construkto3._0.ViewModels
{
    public class AddCounterpartyViewModel : ViewModelBase
    {
        private ObservableCollection<Counterparty> _counterparties;
        public ObservableCollection<Counterparty> Counterparties
        {
            get => _counterparties;
            set => SetProperty(ref _counterparties, value);
        }
        private Counterparty _selectedCounterpartyItem;
        public Counterparty SelectedCounterpartyItem
        {
            get => _selectedCounterpartyItem;
            set => SetProperty(ref _selectedCounterpartyItem, value);
        }
        private Counterparty _selectedCounterparty;
        public Counterparty SelectedCounterparty
        {
            get => _selectedCounterparty;
            set { _selectedCounterparty = value; OnPropertyChanged(nameof(SelectedCounterparty)); }
        }
        public ICommand AddCounterpartyCommand { get; }
        public ICommand DeleteCounterpartyCommand { get; }
        public ICommand UpdateCounterpartyCommand { get; }
   
        public ICommand SelectCounterpartyCommand { get; }
        public AddCounterpartyViewModel() 
        {
            var allItems = DatabaseService.LoadItems() ?? new List<Item>();
            Counterparties = new ObservableCollection<Counterparty>(DatabaseService.LoadCounterparties() ?? new List<Counterparty>());
            AddCounterpartyCommand = new RelayCommand(_ => AddCounterparty());
            DeleteCounterpartyCommand = new RelayCommand(_ => DeleteCounterparty(), _ => SelectedCounterpartyItem != null);
            UpdateCounterpartyCommand = new RelayCommand(_ => UpdateCounterparty(), _ => SelectedCounterpartyItem != null);
            SelectCounterpartyCommand = new RelayCommand(DoSelectCounterparty, _ => SelectedCounterpartyItem != null);
        }
        private void AddCounterparty()
        {
            var newCounterparty = new Counterparty { Name = "Новый контрагент", Address = "Адрес", Contact = "Контакт", Email = "Электронная почта" };
            DatabaseService.AddCounterparty(newCounterparty);
            Counterparties.Add(newCounterparty);
        }
        private void DoSelectCounterparty(object windowObj)
        {
            SelectedCounterparty = SelectedCounterpartyItem;
            if (windowObj is Window wnd)
            {
                wnd.DialogResult = true;
                wnd.Close();
            }
        }
        private void DeleteCounterparty()
        {
            if (SelectedCounterpartyItem != null)
            {
                DatabaseService.DeleteCounterparty(SelectedCounterpartyItem.Id);
                Counterparties.Remove(SelectedCounterpartyItem);
            }
        }

        private void UpdateCounterparty()
        {
            if (SelectedCounterpartyItem != null)
            {
                DatabaseService.UpdateCounterparty(SelectedCounterpartyItem);
            }
        }
    }
}
