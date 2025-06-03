using System.Collections.ObjectModel;
using System.Windows.Input;
using construkto3._0.Models;
using construkto3._0.Services;

namespace construkto3._0.ViewModels
{
    public class DatabaseViewModel : ViewModelBase
    {
        private ObservableCollection<Item> _goodsItems;
        public ObservableCollection<Item> GoodsItems
        {
            get => _goodsItems;
            set => SetProperty(ref _goodsItems, value);
        }

        private ObservableCollection<Item> _servicesItems;
        public ObservableCollection<Item> ServicesItems
        {
            get => _servicesItems;
            set => SetProperty(ref _servicesItems, value);
        }

        private ObservableCollection<Item> _additionalItems;
        public ObservableCollection<Item> AdditionalItems
        {
            get => _additionalItems;
            set => SetProperty(ref _additionalItems, value);
        }


        private Item _selectedGoodsItem;
        public Item SelectedGoodsItem
        {
            get => _selectedGoodsItem;
            set => SetProperty(ref _selectedGoodsItem, value);
        }

        private Item _selectedServicesItem;
        public Item SelectedServicesItem
        {
            get => _selectedServicesItem;
            set => SetProperty(ref _selectedServicesItem, value);
        }

        private Item _selectedAdditionalItem;
        public Item SelectedAdditionalItem
        {
            get => _selectedAdditionalItem;
            set => SetProperty(ref _selectedAdditionalItem, value);
        }


        public ICommand SaveGoodsCommand { get; }
        public ICommand DeleteGoodsCommand { get; }
        public ICommand UpdateGoodsCommand { get; }
        public ICommand SaveServicesCommand { get; }
        public ICommand DeleteServicesCommand { get; }
        public ICommand UpdateServicesCommand { get; }
        public ICommand SaveAdditionalCommand { get; }
        public ICommand DeleteAdditionalCommand { get; }
        public ICommand UpdateAdditionalCommand { get; }
 

        public DatabaseViewModel()
        {
            var allItems = DatabaseService.LoadItems() ?? new List<Item>();
            GoodsItems = new ObservableCollection<Item>(allItems.Where(item => item.Category == "Товары"));
            ServicesItems = new ObservableCollection<Item>(allItems.Where(item => item.Category == "Услуги"));
            AdditionalItems = new ObservableCollection<Item>(allItems.Where(item => item.Category == "Доп. товары"));

            SaveGoodsCommand = new RelayCommand(_ => SaveGoods());
            DeleteGoodsCommand = new RelayCommand(_ => DeleteGoods(), _ => SelectedGoodsItem != null);
            UpdateGoodsCommand = new RelayCommand(_ => UpdateGoods(), _ => SelectedGoodsItem != null);

            SaveServicesCommand = new RelayCommand(_ => SaveServices());
            DeleteServicesCommand = new RelayCommand(_ => DeleteServices(), _ => SelectedServicesItem != null);
            UpdateServicesCommand = new RelayCommand(_ => UpdateServices(), _ => SelectedServicesItem != null);

            SaveAdditionalCommand = new RelayCommand(_ => SaveAdditional());
            DeleteAdditionalCommand = new RelayCommand(_ => DeleteAdditional(), _ => SelectedAdditionalItem != null);
            UpdateAdditionalCommand = new RelayCommand(_ => UpdateAdditional(), _ => SelectedAdditionalItem != null);
        }

        private void SaveGoods()
        {
            var newItem = new Item { Category = "Товары", Name = "Новый товар", UnitPrice = 0m };
            DatabaseService.AddItem(newItem);
            GoodsItems.Add(newItem);
        }

        private void DeleteGoods()
        {
            if (SelectedGoodsItem != null)
            {
                DatabaseService.DeleteItem(SelectedGoodsItem);
                GoodsItems.Remove(SelectedGoodsItem);
            }
        }

        private void UpdateGoods()
        {
            if (SelectedGoodsItem != null)
            {
                DatabaseService.UpdateItem(SelectedGoodsItem);
            }
        }

        private void SaveServices()
        {
            var newItem = new Item { Category = "Услуги", Name = "Новая услуга", UnitPrice = 0m };
            DatabaseService.AddItem(newItem);
            ServicesItems.Add(newItem);
        }

        private void DeleteServices()
        {
            if (SelectedServicesItem != null)
            {
                DatabaseService.DeleteItem(SelectedServicesItem);
                ServicesItems.Remove(SelectedServicesItem);
            }
        }

        private void UpdateServices()
        {
            if (SelectedServicesItem != null)
            {
                DatabaseService.UpdateItem(SelectedServicesItem);
            }
        }

        private void SaveAdditional()
        {
            var newItem = new Item { Category = "Доп. товары", Name = "Новый доп. товар", UnitPrice = 0m };
            DatabaseService.AddItem(newItem);
            AdditionalItems.Add(newItem);
        }

        private void DeleteAdditional()
        {
            if (SelectedAdditionalItem != null)
            {
                DatabaseService.DeleteItem(SelectedAdditionalItem);
                AdditionalItems.Remove(SelectedAdditionalItem);
            }
        }

        private void UpdateAdditional()
        {
            if (SelectedAdditionalItem != null)
            {
                DatabaseService.UpdateItem(SelectedAdditionalItem);
            }
        }      
    }
}