using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace construkto3._0.aiserver
{
    public class G4FServerManager
    {
        private const string Host = "127.0.0.1";
        private const int Port = 5000;
        private const string ExeName = "g4f_server.exe";

        /// <summary>
        /// Проверяет, открыт ли нужный порт на локалхосте.
        /// </summary>
        private bool IsPortOpen(string host, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var task = client.ConnectAsync(host, port);
                    return task.Wait(500) && client.Connected;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, запущен ли g4f сервер (по порту).
        /// </summary>
        public bool IsServerRunning()
        {
            return IsPortOpen(Host, Port);
        }

        /// <summary>
        /// Запускает сервер, если он не работает.
        /// </summary>
        public void StartServerIfNotRunning()
        {
            if (IsServerRunning())
                return;

            string exePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ExeName);
            if (!System.IO.File.Exists(exePath))
            {
                MessageBox.Show($"{ExeName} не найден! Помести его рядом с программой.", "Ошибка запуска g4f", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exePath,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                Process.Start(psi);

                // Даем серверу время стартовать (до 10 секунд)
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(500);
                    if (IsServerRunning())
                        return;
                }

                MessageBox.Show("g4f_server.exe был запущен, но порт 5000 не открылся вовремя. Проверьте сервер вручную.", "Ошибка запуска g4f", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось запустить {ExeName}: {ex.Message}", "Ошибка запуска g4f", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}