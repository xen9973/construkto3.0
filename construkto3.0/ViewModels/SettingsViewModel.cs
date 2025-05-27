using System;
using System.IO;
using System.Windows;
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

        public SettingsViewModel()
        {
            // Загружаем существующие данные при старте, если есть
            LoadExistingData();
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
                }
            }
            if (File.Exists(imagePath))
            {
                UserImagePath = imagePath;
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
                File.WriteAllText(userDataFile, $"{NewCounterparty.Name}\n{NewCounterparty.Address}\n{NewCounterparty.Contact}");
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
                UserImagePath = openFileDialog.FileName; // Пока просто сохраняем путь, копия будет при сохранении
            }
        }
    }
}