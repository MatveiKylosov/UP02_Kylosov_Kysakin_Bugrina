using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UP02.Context;
using UP02.Helpers;
using UP02.Interfaces;
using UP02.Models;

namespace UP02.Elements
{
    /// <summary>
    /// Логика взаимодействия для ItemNetworkSettings.xaml
    /// </summary>
    public partial class ItemNetworkSettings : UserControl, IRecordDeletable, IRecordUpdatable
    {
        public event EventHandler RecordDelete;        public event EventHandler RecordUpdate;
        private NetworkSettings NetworkSettings;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ItemNetworkSettings"/>.
        /// </summary>
        /// <param name="NetworkSettings">Настройки сети для отображения.</param>
        public ItemNetworkSettings(NetworkSettings NetworkSettings)
        {
            InitializeComponent();
            this.NetworkSettings = NetworkSettings;
            this.DataContext = NetworkSettings;
        }

        /// <summary>
        /// Обработчик клика по кнопке обновления настроек сети.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new Pages.Elements.EditNetworkSettings(NetworkSettings);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик события успешного редактирования настроек сети.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.NetworkSettings = sender as NetworkSettings;
            this.DataContext = this.NetworkSettings;
            RecordUpdate?.Invoke(NetworkSettings, EventArgs.Empty);
        }

        /// <summary>
        /// Обработчик клика по кнопке удаления настроек сети.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                // Находим и удаляем настройки сети из базы данных
                var networkSettings = databaseContext.NetworkSettings.FirstOrDefault(x => x.NetworkID == NetworkSettings.NetworkID);
                if (networkSettings != null)
                {
                    databaseContext.NetworkSettings.Remove(networkSettings);
                    databaseContext.SaveChanges();
                }

                // Сигнализируем об удалении записи
                RecordDelete?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                // Обработка ошибок при подключении
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }

        /// <summary>
        /// Обработчик события удаления записи.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void _RecordDelete(object sender, EventArgs e)
        {
            RecordDelete?.Invoke(this, e);
        }

        /// <summary>
        /// Обработчик клика по кнопке проверки подключения к серверу.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private async void CheckConnection(object sender, RoutedEventArgs e)
        {
            string server = NetworkSettings.IPAddress;
            int port = 1337;

            ConnectionProgressBar.Visibility = Visibility.Visible;

            TcpClient tcpClient = new TcpClient();
            Task connectTask = tcpClient.ConnectAsync(server, port);
            Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));

            if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
            {
                MessageBox.Show("Сервер не доступен");
                ConnectionProgressBar.Visibility = Visibility.Collapsed;
                return;
            }

            ConnectionProgressBar.Visibility = Visibility.Collapsed;
            MessageBox.Show("Подключение успешно установлено");
        }
    }
}
