using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using construkto3._0.Models;
using construkto3._0.Services;
using Microsoft.Win32;

namespace construkto3._0.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<Item> AvailableItems { get; }
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

        public ICommand GenerateCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SaveCommand { get; }

        public MainViewModel()
        {
            AvailableItems = new ObservableCollection<Item>(DatabaseService.LoadItems());
            SelectedItems = new ObservableCollection<Item>();
            Counterparties = new ObservableCollection<Counterparty>(DatabaseService.LoadCounterparties());

            GenerateCommand = new RelayCommand(_ => GenerateProposal(), _ => SelectedCounterparty != null && SelectedItems.Any());
            AddCommand = new RelayCommand(_ => AddSelectedItem(), _ => SelectedAvailable != null);
            RemoveCommand = new RelayCommand(_ => RemoveSelectedItem(), _ => SelectedChosen != null);
            SaveCommand = new RelayCommand(_ => SaveToRtf(), _ => !string.IsNullOrWhiteSpace(GeneratedText));
        }

        private void SaveToRtf()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Rich Text Format (*.rtf)|*.rtf",
                FileName = "Коммерческое предложение.rtf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    var flowDocument = new FlowDocument();
                    // Разделяем текст на строки и создаём отдельный Paragraph для каждой строки
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

        private void AddSelectedItem()
        {
            if (SelectedAvailable != null)
            {
                var item = SelectedAvailable;
                AvailableItems.Remove(item);
                item.Quantity = 1; // устанавливаем количество по умолчанию
                SelectedItems.Add(item);
                SelectedAvailable = null;
            }
        }

        private void RemoveSelectedItem()
        {
            if (SelectedChosen != null)
            {
                var item = SelectedChosen;
                SelectedItems.Remove(item);
                AvailableItems.Add(item);
                SelectedChosen = null;
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

            // Генерация текста для RichTextBox
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

            // Привязка к RichTextBox
            GeneratedText = sb.ToString();
        }
    }
}