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
using System.Windows.Media.Imaging;
using System.Windows.Controls;

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
            set
            {
                SetProperty(ref _generatedText, value);
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
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
        public RichTextBox MainRichTextBox { get; set; }

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
            SaveCommand = new RelayCommand(_ => SaveToRtf(), _ => true);
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
                Filter = "Rich Text Format (.rtf)|*.rtf|PDF Document (.pdf)|*.pdf|Word Document (.docx)|*.docx",
                FileName = "Коммерческое предложение"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Здесь MainRichTextBox — твой RichTextBox (с логотипом и форматированием)
                if (MainRichTextBox == null)
                {
                    MessageBox.Show("Документ для сохранения не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    // Сохраняем весь текущий документ с картинками и форматированием
                    TextRange textRange = new TextRange(
                        MainRichTextBox.Document.ContentStart,
                        MainRichTextBox.Document.ContentEnd);

                    // Проверяем выбранный формат
                    string ext = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                    if (ext == ".rtf")
                    {
                        textRange.Save(fileStream, DataFormats.Rtf);
                    }
                    else if (ext == ".docx")
                    {
                        textRange.Save(fileStream, DataFormats.XamlPackage); // Word откроет XamlPackage
                    }
                    else if (ext == ".pdf")
                    {
                        MessageBox.Show("Сохранение в PDF не поддерживается стандартными средствами WPF.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        // Можно реализовать через сторонние библиотеки, например, MigraDoc, PdfSharp, iTextSharp и т.д.
                    }
                    else
                    {
                        textRange.Save(fileStream, DataFormats.Rtf);
                    }
                }
            }
        }
        private Counterparty LoadSupplierFromSettings()
        {
            string userDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userData");
            string userDataFile = Path.Combine(userDataPath, "userData.txt");

            if (File.Exists(userDataFile))
            {
                var lines = File.ReadAllLines(userDataFile);
                // Ожидается: Name, Address, Contact, INN, KPP, Phone, Email (по строкам)
                return new Counterparty
                {
                    Name = lines.Length > 0 ? lines[0] : "",
                    Address = lines.Length > 1 ? lines[1] : "",
                    Contact = lines.Length > 2 ? lines[2] : "",
                    INN = lines.Length > 3 ? lines[3] : "",
                    KPP = lines.Length > 4 ? lines[4] : "",
                    Phone = lines.Length > 5 ? lines[5] : "",
                    Email = lines.Length > 6 ? lines[6] : ""
                };
            }
            return new Counterparty();
        }
        private void GenerateProposal()
        {
            var supplier = LoadSupplierFromSettings();

            if (SelectedCounterparty == null)
            {
                MessageBox.Show("Выберите контрагента.");
                return;
            }
            if (!SelectedItems.Any())
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один товар или услугу.");
                return;
            }

            var flowDocument = new FlowDocument();

            // === 1. ЛОГОТИП ===
            string userDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userData");
            string[] possibleExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };
            string logoPath = possibleExtensions
                .Select(ext => Path.Combine(userDataFolder, "userImage" + ext))
                .FirstOrDefault(File.Exists);
            if (File.Exists(logoPath))
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(logoPath, UriKind.Absolute)),
                    Width = 150,
                    Height = 60,
                    Margin = new Thickness(0, 0, 0, 18)
                };
                flowDocument.Blocks.Add(new BlockUIContainer(image) { Margin = new Thickness(0, 0, 0, 10) });
            }

            // === 2. ТЕКСТ КП ===
            flowDocument.Blocks.Add(new Paragraph(new Bold(new Run($"Коммерческое предложение № 001"))));
            flowDocument.Blocks.Add(new Paragraph(new Run($"от {DateTime.Now:dd.MM.yyyy}")));

            var supplierBlock = new Paragraph();
            supplierBlock.Inlines.Add(new Bold(new Run("Поставщик: ")));
            supplierBlock.Inlines.Add(new Run($"{supplier.Name}"));
            supplierBlock.Inlines.Add(new LineBreak());
            supplierBlock.Inlines.Add(new Run($"ИНН/КПП: {supplier.INN}/{supplier.KPP}"));
            supplierBlock.Inlines.Add(new LineBreak());
            supplierBlock.Inlines.Add(new Run($"Адрес: {supplier.Address}"));
            supplierBlock.Inlines.Add(new LineBreak());
            supplierBlock.Inlines.Add(new Run($"Телефон: {supplier.Phone}"));
            supplierBlock.Inlines.Add(new LineBreak());
            supplierBlock.Inlines.Add(new Run($"Email: {supplier.Email}"));
            supplierBlock.Inlines.Add(new LineBreak());
            flowDocument.Blocks.Add(supplierBlock);

            var buyerBlock = new Paragraph();
            buyerBlock.Inlines.Add(new Bold(new Run("Покупатель: ")));
            buyerBlock.Inlines.Add(new Run($"{SelectedCounterparty.Name}"));
            buyerBlock.Inlines.Add(new LineBreak());
            buyerBlock.Inlines.Add(new Run($"Адрес: {SelectedCounterparty.Address}"));
            buyerBlock.Inlines.Add(new LineBreak());
            buyerBlock.Inlines.Add(new Run($"Контактное лицо: {SelectedCounterparty.Contact}"));
            supplierBlock.Inlines.Add(new LineBreak());
            flowDocument.Blocks.Add(buyerBlock);

            // === 3. ТАБЛИЦА ПОЗИЦИЙ ===
            void AddSection(string title, string category)
            {
                var sectionItems = SelectedItems.Where(i => i.Category == category).ToList();
                var p = new Paragraph(new Bold(new Run(title)));
                flowDocument.Blocks.Add(p);

                if (sectionItems.Any())
                {
                    int idx = 1;
                    foreach (var it in sectionItems)
                    {
                        decimal total = it.UnitPrice * it.Quantity;
                        flowDocument.Blocks.Add(new Paragraph(
                            new Run($"{idx++}. {it.Name} – {it.Quantity} шт. × {it.UnitPrice:N2} = {total:N2}")
                        ));
                    }
                }
                else
                {
                    flowDocument.Blocks.Add(new Paragraph(new Run("(нет позиций)\n")));
                }
                flowDocument.Blocks.Add(new Paragraph());
            }
            AddSection("I. Оборудование", "Товары");
            AddSection("II. Программа", "Услуги");
            AddSection("III. Доп.товары", "Доп. товары");

            decimal Sum(string cat) => SelectedItems
                .Where(i => i.Category == cat)
                .Sum(i => i.UnitPrice * i.Quantity);

            flowDocument.Blocks.Add(new Paragraph(new Run($"Итого «Оборудование»: {Sum("Товары"):N2}")));
            flowDocument.Blocks.Add(new Paragraph(new Run($"Итого «Программа»: {Sum("Услуги"):N2}")));
            flowDocument.Blocks.Add(new Paragraph(new Run($"Итого «Доп.товары»: {Sum("Доп. товары"):N2}\n")));

            decimal grand = SelectedItems.Sum(i => i.UnitPrice * i.Quantity);
            flowDocument.Blocks.Add(new Paragraph(new Bold(new Run($"Общая сумма: {grand:N2}\n"))));

            flowDocument.Blocks.Add(new Paragraph(new Run("С уважением,\n")));
            flowDocument.Blocks.Add(new Paragraph(new Run("____________________     ____________________\n(ФИО, Должность)         (ФИО, Должность)\nМ.П.")));

            // === 4. ПОКАЗАТЬ В RichTextBox ===
            if (MainRichTextBox != null)
                MainRichTextBox.Document = flowDocument;
        }
    }
}