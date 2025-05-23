using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using construkto3._0.ViewModels;
using Newtonsoft.Json;
using construkto3._0.Models;
using System.Windows.Input;

namespace construkto3._0.Views
{
    public partial class MainView : Window
    {
        private bool _isDarkTheme = false;
        public MainView()
        {
            InitializeComponent();
            var fontFamilies = Fonts.SystemFontFamilies;
            foreach (var font in fontFamilies)
            {
                FontComboBox.Items.Add(font.Source);
            }
            // Устанавливаем VM как DataContext для биндингов
            this.DataContext = new MainViewModel();
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;

                    Point mousePosition = e.GetPosition(this);
                    Point screenPosition = PointToScreen(mousePosition);

                    this.Left = screenPosition.X - screenPosition.X;
                    this.Top = screenPosition.Y - screenPosition.Y;
                }
                this.DragMove();
            }
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RichTextBoxEditor == null || RichTextBoxEditor.Selection.IsEmpty) return;

            var comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                RichTextBoxEditor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(comboBox.SelectedItem.ToString()));
            }
        }

        private void AlignButton_Click(object sender, RoutedEventArgs e)
        {
            if (RichTextBoxEditor == null || RichTextBoxEditor.Selection.IsEmpty) return;

            var button = sender as Button;
            if (button != null)
            {
                var alignment = (TextAlignment)Enum.Parse(typeof(TextAlignment), button.CommandParameter.ToString());
                RichTextBoxEditor.Selection.Start.Paragraph.TextAlignment = alignment;
            }
        }
        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            if (RichTextBoxEditor == null || RichTextBoxEditor.Selection.IsEmpty) return;

            // Получаем текущий стиль текста
            var currentFontWeight = RichTextBoxEditor.Selection.GetPropertyValue(TextElement.FontWeightProperty);
            var newFontWeight = currentFontWeight.Equals(FontWeights.Bold) ? FontWeights.Normal : FontWeights.Bold;

            RichTextBoxEditor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, newFontWeight);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddCounterpartyView addCounterpartyView = new AddCounterpartyView();
            addCounterpartyView.Show();
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(_isDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
        }

        private void SelectTemplate_Click(object sender, RoutedEventArgs e)
        {
            var templateSelectionWindow = new TemplateSelectionView(selectedTemplate =>
            {
                if (!string.IsNullOrEmpty(selectedTemplate))
                {
                    SelectedTemplateLabel.Content = $"{selectedTemplate}";

                    var viewModel = (MainViewModel)DataContext;
                    var selectedItems = viewModel.SelectedItems;

                    string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", $"{selectedTemplate}.json");
                    if (File.Exists(templatePath))
                    {
                        try
                        {
                            string jsonContent = File.ReadAllText(templatePath);
                            var template = JsonConvert.DeserializeObject<Template>(jsonContent);

                            string result = $"{template.Header}\n";
                            result += $"{template.CompanyInfo}\n";
                            result += $"{template.Introduction}\n";
                            result += "Список товаров:\n";
                            int index = 1;
                            foreach (var item in selectedItems)
                            {
                                string itemDescription = $"Стандартный товар"; // Можно добавить логику для динамического описания
                                string subtotal = (item.Quantity * item.UnitPrice).ToString();
                                string itemLine = template.ItemFormat
                                    .Replace("{Index}", index.ToString())
                                    .Replace("{Name}", item.Name ?? "")
                                    .Replace("{Category}", item.Category ?? "")
                                    .Replace("{Quantity}", item.Quantity.ToString())
                                    .Replace("{UnitPrice}", item.UnitPrice.ToString())
                                    .Replace("{Subtotal}", subtotal)
                                    .Replace("{Description}", itemDescription);
                                result += itemLine + "\n";
                                index++;
                            }
                            decimal total = selectedItems.Sum(item => item.UnitPrice * item.Quantity);
                            decimal discount = total >= 100000 ? (total * template.DefaultDiscount / 100) : 0;
                            decimal totalWithDiscount = total - discount;

                            result += "\n" + template.DeliveryTerms + "\n\n";
                            result += template.AdditionalServices + "\n\n";
                            result += template.FooterFormat
                                .Replace("{Total}", total.ToString())
                                .Replace("{Discount}", template.DefaultDiscount.ToString())
                                .Replace("{TotalWithDiscount}", totalWithDiscount.ToString());

                            viewModel.GeneratedText = result;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при обработке шаблона: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Выбранный шаблон не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });

            templateSelectionWindow.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Minimize_click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void NewData_Click(object sender, RoutedEventArgs e)
        {
            DatabaseView databaseView = new DatabaseView();
            databaseView.ShowDialog();
           
        }

        private void infoExcel_Click(object sender, RoutedEventArgs e)
        { 
            MessageBox.Show("При нажатии кнопки плюс у вас откроется Excel файл в котором вы сможете добавлять свои данные и они потом отобразятся в программе.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("При нажатии кнопки плюс у вас откроется окно в котором вы сможете добавлять свои данные и они потом отобразятся в программе.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

