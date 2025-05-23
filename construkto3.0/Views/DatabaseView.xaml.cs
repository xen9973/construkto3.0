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
    /// <summary>
    /// Логика взаимодействия для DatabaseView.xaml
    /// </summary>
    public partial class DatabaseView : Window
    {
        private bool _isDarkTheme = false;
        public DatabaseView()
        {
            InitializeComponent();
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
        
        private void Minimize_click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
