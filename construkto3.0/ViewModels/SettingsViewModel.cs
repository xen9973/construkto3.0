using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using construkto3._0.Models;
using Microsoft.Win32;

namespace construkto3._0.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private Counterparty _newCounterparty = new Counterparty();
        public Counterparty NewCounterparty
        {
            get => _newCounterparty;
            set
            {
                _newCounterparty = value;
                OnPropertyChanged(nameof(NewCounterparty));
            }
        }

        private string _userImagePath;
        public string UserImagePath
        {
            get => _userImagePath;
            set
            {
                _userImagePath = value;
                OnPropertyChanged(nameof(UserImagePath));
            }
        }
        private BitmapImage _userImagePreview;
        public BitmapImage UserImagePreview
        {
            get => _userImagePreview;
            set { _userImagePreview = value; OnPropertyChanged(nameof(UserImagePreview)); }
        }

        public SettingsViewModel()
        {
            // Загружаем существующие данные при старте, если есть
            LoadExistingData();
        }

        private BitmapImage LoadImageWithoutLock(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        private void LoadExistingData()
        {
            string userDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userData");
            string userDataFile = Path.Combine(userDataPath, "userData.txt");
            string imagePath = Path.Combine(userDataPath, "userImage.png");

            if (File.Exists(userDataFile))
            {
                var lines = File.ReadAllLines(userDataFile);
                if (lines.Length >= 3)
                {
                    NewCounterparty.Name = lines[0];
                    NewCounterparty.Address = lines[1];
                    NewCounterparty.Contact = lines[2];
                    NewCounterparty.INN = lines[3];
                    NewCounterparty.KPP = lines[4];
                    NewCounterparty.Phone = lines[5];
                    NewCounterparty.Email = lines[6];
                }
            }
            if (File.Exists(imagePath))
            {
                UserImagePath = imagePath;
                try
                {
                    UserImagePreview = LoadImageWithoutLock(UserImagePath);
                }
                catch
                {
                    UserImagePreview = null;
                }
            }
        }

        public void SaveData()
        {
            string userDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userData");
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }

            string userDataFile = Path.Combine(userDataPath, "userData.txt");
            try
            {
                // Теперь сохраняем все 7 строк: Name, Address, Contact, INN, KPP, Phone, Email
                File.WriteAllText(userDataFile,
                    $"{NewCounterparty.Name}\n" +
                    $"{NewCounterparty.Address}\n" +
                    $"{NewCounterparty.Contact}\n" +
                    $"{NewCounterparty.INN}\n" +
                    $"{NewCounterparty.KPP}\n" +
                    $"{NewCounterparty.Phone}\n" +
                    $"{NewCounterparty.Email}");

                if (!string.IsNullOrEmpty(UserImagePath) && File.Exists(UserImagePath))
                {
                    string targetImagePath = Path.Combine(userDataPath, "userImage.png");
                    File.Copy(UserImagePath, targetImagePath, true); // Перезаписываем изображение
                }
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
                Title = "Выберите изображение"
            };
                if (openFileDialog.ShowDialog() == true)
                {
                    UserImagePath = openFileDialog.FileName;
                    UserImagePreview = LoadImageWithoutLock(UserImagePath);
                } // Пока просто сохраняем путь, копия будет при сохранении
            
        }
    }
}