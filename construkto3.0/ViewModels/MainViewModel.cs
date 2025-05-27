using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ClosedXML.Excel;
using construkto3._0.Models;
using construkto3._0.Services;
using Microsoft.Win32;
using System.Diagnostics;

namespace construkto3._0.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<Item> _databaseItems;
        public ObservableCollection<Item> DatabaseItems
        {
            get => _databaseItems;
            set => SetProperty(ref _databaseItems, value);
        }

        private ObservableCollection<Item> _filteredDatabaseItems = new ObservableCollection<Item>();
        public ObservableCollection<Item> FilteredDatabaseItems
        {
            get => _filteredDatabaseItems;
            set => SetProperty(ref _filteredDatabaseItems, value);
        }

        private ObservableCollection<Item> _availableItems;
        public ObservableCollection<Item> AvailableItems
        {
            get => _availableItems;
            set => SetProperty(ref _availableItems, value);
        }

        private ObservableCollection<Item> _filteredAvailableItems = new ObservableCollection<Item>();
        public ObservableCollection<Item> FilteredAvailableItems
        {
            get => _filteredAvailableItems;
            set => SetProperty(ref _filteredAvailableItems, value);
        }

        public ObservableCollection<Item> SelectedItems { get; }
        public ObservableCollection<Counterparty> Counterparties { get; }

        private Counterparty _selectedCounterparty;
        public Counterparty SelectedCounterparty
        {
            get => _selectedCounterparty;
            set => SetProperty(ref _selectedCounterparty, value);
        }

        private Item _selectedAvailable;
        public Item SelectedAvailable
        {
            get => _selectedAvailable;
            set => SetProperty(ref _selectedAvailable, value);
        }

        private Item _selectedChosen;
        public Item SelectedChosen
        {
            get => _selectedChosen;
            set => SetProperty(ref _selectedChosen, value);
        }

        private string _generatedText;
        public string GeneratedText
        {
            get => _generatedText;
            set => SetProperty(ref _generatedText, value);
        }

        private ObservableCollection<string> _availableCategories = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableCategories
        {
            get => _availableCategories;
            set => SetProperty(ref _availableCategories, value);
        }

        private ObservableCollection<string> _excelCategories = new ObservableCollection<string>();
        public ObservableCollection<string> ExcelCategories
        {
            get => _excelCategories;
            set => SetProperty(ref _excelCategories, value);
        }

        private string _selectedAvailableCategory;
        public string SelectedAvailableCategory
        {
            get => _selectedAvailableCategory;
            set
            {
                if (_selectedAvailableCategory != value)
                {
                    _selectedAvailableCategory = value;
                    OnPropertyChanged(nameof(SelectedAvailableCategory));
                    ApplyDatabaseItemsFilter();
                }
            }
        }

        private string _selectedExcelCategory;
        public string SelectedExcelCategory
        {
            get => _selectedExcelCategory;
            set
            {
                if (_selectedExcelCategory != value)
                {
                    _selectedExcelCategory = value;
                    OnPropertyChanged(nameof(SelectedExcelCategory));
                    ApplyAvailableItemsFilter();
                }
            }
        }

        private bool _isUpdatingCategories = false;

        public ICommand GenerateCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddExcelCommand { get; }
        public ICommand UpdateExcelCommand { get; }
        public ICommand RefreshDatabaseCommand { get; }

        public MainViewModel()
        {
            SelectedItems = new ObservableCollection<Item>();
            Counterparties = new ObservableCollection<Counterparty>(DatabaseService.LoadCounterparties() ?? new List<Counterparty>());

            AvailableCategories = new ObservableCollection<string>();
            AvailableCategories.Insert(0, "Все категории");
            ExcelCategories = new ObservableCollection<string>();
            ExcelCategories.Insert(0, "Все категории");
            _selectedAvailableCategory = "Все категории";
            _selectedExcelCategory = "Все категории";

            GenerateCommand = new RelayCommand(_ => GenerateProposal(), _ => SelectedCounterparty != null && SelectedItems.Any());
            AddCommand = new RelayCommand(_ => AddSelectedItem(), _ => SelectedAvailable != null);
            RemoveCommand = new RelayCommand(_ => RemoveSelectedItem(), _ => SelectedChosen != null);
            SaveCommand = new RelayCommand(_ => SaveToRtf(), _ => !string.IsNullOrWhiteSpace(GeneratedText));
            AddExcelCommand = new RelayCommand(_ => AddExcelData());
            UpdateExcelCommand = new RelayCommand(_ => UpdateExcelData());
            RefreshDatabaseCommand = new RelayCommand(_ => RefreshDatabase());

            DatabaseItems = new ObservableCollection<Item>(DatabaseService.LoadItems() ?? new List<Item>());
            FilteredDatabaseItems = new ObservableCollection<Item>(DatabaseItems);
            ApplyDatabaseItemsFilter();

            AvailableItems = new ObservableCollection<Item>();
            FilteredAvailableItems = new ObservableCollection<Item>();
            ApplyAvailableItemsFilter();
        }
        private void RefreshDatabase()
        {
            // Перезагружаем данные из базы данных
            DatabaseItems.Clear();
            var items = DatabaseService.LoadItems() ?? new List<Item>();
            foreach (var item in items)
            {
                DatabaseItems.Add(item);
            }

            Counterparties.Clear();
            var counterparties = DatabaseService.LoadCounterparties() ?? new List<Counterparty>();
            foreach (var counterparty in counterparties)
            {
                Counterparties.Add(counterparty);
            }

            // Перезапускаем фильтрацию, чтобы обновить отображаемые данные
            ApplyDatabaseItemsFilter();
            ApplyAvailableItemsFilter();
        }

        private void AddSelectedItem()
        {
            if (SelectedAvailable != null)
            {
                var item = SelectedAvailable.Clone() as Item;
                item.Quantity = 1;

                if (DatabaseItems.Contains(SelectedAvailable))
                {
                    DatabaseItems.Remove(SelectedAvailable);
                    ApplyDatabaseItemsFilter(); // Обновляем категории для DatabaseItems
                }
                else if (AvailableItems.Contains(SelectedAvailable))
                {
                    AvailableItems.Remove(SelectedAvailable);
                    ApplyAvailableItemsFilter(); // Обновляем категории для AvailableItems
                }

                SelectedItems.Add(item);

                _selectedAvailable = null;
                OnPropertyChanged(nameof(SelectedAvailable));
            }
        }

        private void RemoveSelectedItem()
        {
            if (SelectedChosen != null)
            {
                var item = SelectedChosen;
                SelectedItems.Remove(item);
                if (item.Source.StartsWith("Sheet"))
                {
                    AvailableItems.Add(item);
                    ApplyAvailableItemsFilter(); // Обновляем категории для AvailableItems
                }
                else
                {
                    DatabaseItems.Add(item);
                    ApplyDatabaseItemsFilter(); // Обновляем категории для DatabaseItems
                }
                SelectedChosen = null;
            }
        }

        private void AddExcelData()
        {
            try
            {
                string excelFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exceldb");
                if (!Directory.Exists(excelFolderPath))
                {
                    MessageBox.Show($"Папка Exceldb не найдена по пути: {excelFolderPath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                System.Diagnostics.Process.Start("explorer.exe", excelFolderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии папки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateExcelData()
        {
            try
            {
                string excelFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exceldb");
                if (!Directory.Exists(excelFolderPath))
                {
                    MessageBox.Show($"Папка Exceldb не найдена по пути: {excelFolderPath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string[] excelFiles = Directory.GetFiles(excelFolderPath, "*.xlsx");
                if (excelFiles.Length == 0)
                {
                    MessageBox.Show($"В папке {excelFolderPath} не найдено ни одного файла .xlsx", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string excelFilePath = excelFiles[0];

                using (var workbook = new ClosedXML.Excel.XLWorkbook(excelFilePath))
                {
                    AvailableItems.Clear();

                    var sheets = new[] { "Sheet1", "Sheet2", "Sheet3" };
                    foreach (var sheetName in sheets)
                    {
                        var worksheet = workbook.Worksheet(sheetName);
                        if (worksheet != null)
                        {
                            for (int row = 2; row <= worksheet.LastRowUsed().RowNumber(); row++)
                            {
                                var item = new Item
                                {
                                    Name = worksheet.Cell(row, 1).GetValue<string>().Trim(),
                                    Category = worksheet.Cell(row, 2).GetValue<string>().Trim(),
                                    UnitPrice = worksheet.Cell(row, 3).GetValue<decimal?>().GetValueOrDefault(0m),
                                    Source = sheetName
                                };
                                if (!string.IsNullOrEmpty(item.Name))
                                {
                                    AvailableItems.Add(item);
                                }
                            }
                        }
                    }

                    // Обновляем категории только для Excel
                    var excelCategories = new ObservableCollection<string>(AvailableItems?.Select(item => item.Category).Distinct() ?? Enumerable.Empty<string>());
                    excelCategories.Insert(0, "Все категории");
                    ExcelCategories = excelCategories;
                    _selectedExcelCategory = "Все категории";
                    OnPropertyChanged(nameof(SelectedExcelCategory));
                    ApplyAvailableItemsFilter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных из Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyDatabaseItemsFilter()
        {
            if (_isUpdatingCategories) return;

            FilteredDatabaseItems.Clear();
            if (DatabaseItems == null || !DatabaseItems.Any())
            {
                return;
            }

            var filtered = DatabaseItems
                .Where(item => (SelectedAvailableCategory == "Все категории" || item.Category == SelectedAvailableCategory));
            foreach (var item in filtered)
            {
                FilteredDatabaseItems.Add(item);
            }

            _isUpdatingCategories = true;
            try
            {
                var newCategories = new ObservableCollection<string>(DatabaseItems?.Select(item => item.Category).Distinct() ?? Enumerable.Empty<string>());
                newCategories.Insert(0, "Все категории");

                if (!newCategories.Contains(_selectedAvailableCategory))
                {
                    _selectedAvailableCategory = "Все категории";
                    OnPropertyChanged(nameof(SelectedAvailableCategory));
                }

                AvailableCategories = newCategories;
            }
            finally
            {
                _isUpdatingCategories = false;
            }
        }

        private void ApplyAvailableItemsFilter()
        {
            if (_isUpdatingCategories) return;

            FilteredAvailableItems.Clear();
            if (AvailableItems == null || !AvailableItems.Any())
            {
                return;
            }

            var filtered = AvailableItems
                .Where(item => (SelectedExcelCategory == "Все категории" || item.Category == SelectedExcelCategory));
            foreach (var item in filtered)
            {
                FilteredAvailableItems.Add(item);
            }

            // Обновляем категории для Excel
            var excelCategories = new ObservableCollection<string>(AvailableItems?.Select(item => item.Category).Distinct() ?? Enumerable.Empty<string>());
            excelCategories.Insert(0, "Все категории");
            if (!excelCategories.Contains(_selectedExcelCategory))
            {
                _selectedExcelCategory = "Все категории";
                OnPropertyChanged(nameof(SelectedExcelCategory));
            }
            ExcelCategories = excelCategories;
        }

        private void SaveToRtf()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Rich Text Format (.rtf)|.rtf|PDF Document (.pdf)|.pdf|Word Document (.docx)|.docx",
                FileName = "Коммерческое предложение"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    var flowDocument = new FlowDocument();
                    var lines = GeneratedText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var paragraph = new Paragraph(new Run(line.Trim()));
                        flowDocument.Blocks.Add(paragraph);
                    }

                    TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                    textRange.Save(fileStream, DataFormats.Rtf);
                }
            }
        }

        private void GenerateProposal()
        {
            var supplier = new Supplier
            {
                Name = "ООО Рога и Копыта",
                INN = "1234567890",
                KPP = "987654321",
                Address = "г. Москва, ул. Примерная, 1",
                Phone = "+7 (495) 123-45-67",
                Email = "info@example.com"
            };

            if (SelectedCounterparty == null)
            {
                GeneratedText = "Выберите контрагента.";
                return;
            }

            if (!SelectedItems.Any())
            {
                GeneratedText = "Пожалуйста, выберите хотя бы один товар или услугу.";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Коммерческое предложение № 001");
            sb.AppendLine($"от {DateTime.Now:dd.MM.yyyy}\n");

            sb.AppendLine($"Поставщик: {supplier.Name}");
            sb.AppendLine($"ИНН/КПП: {supplier.INN}/{supplier.KPP}");
            sb.AppendLine($"Адрес: {supplier.Address}");
            sb.AppendLine($"Телефон: {supplier.Phone}");
            sb.AppendLine($"Email: {supplier.Email}\n");

            sb.AppendLine($"Покупатель: {SelectedCounterparty.Name}");
            sb.AppendLine($"Адрес: {SelectedCounterparty.Address}");
            sb.AppendLine($"Контактное лицо: {SelectedCounterparty.Contact}\n");

            void AppendSection(string title, string category)
            {
                sb.AppendLine(title);
                var sectionItems = SelectedItems.Where(i => i.Category == category).ToList();
                if (!sectionItems.Any())
                {
                    sb.AppendLine("(нет позиций)\n");
                    return;
                }
                int idx = 1;
                foreach (var it in sectionItems)
                {
                    decimal total = it.UnitPrice * it.Quantity;
                    sb.AppendLine($"{idx++}. {it.Name} – {it.Quantity} шт. × {it.UnitPrice:N2} = {total:N2}");
                }
                sb.AppendLine();
            }

            AppendSection("I. Оборудование", "Товары");
            AppendSection("II. Программа", "Услуги");
            AppendSection("III. Доп.товары", "Доп. товары");

            decimal Sum(string cat) => SelectedItems
                .Where(i => i.Category == cat)
                .Sum(i => i.UnitPrice * i.Quantity);

            sb.AppendLine($"Итого «Программа»: {Sum("Товары"):N2}");
            sb.AppendLine($"Итого «Оборудование»: {Sum("Услуги"):N2}");
            sb.AppendLine($"Итого «Доп.товары»: {Sum("Доп. товары"):N2}\n");

            decimal grand = SelectedItems.Sum(i => i.UnitPrice * i.Quantity);
            sb.AppendLine($"Общая сумма: {grand:N2}\n");

            sb.AppendLine("С уважением,\n");
            sb.AppendLine("____________________     ____________________");
            sb.AppendLine("(ФИО, Должность)         (ФИО, Должность)");
            sb.AppendLine("М.П.");

            GeneratedText = sb.ToString();
        }
    }
}