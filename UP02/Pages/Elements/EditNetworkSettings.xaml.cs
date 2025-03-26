using System;
using System.Collections.Generic;
using System.Linq;
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
using UP02.Models;

namespace UP02.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для EditNetworkSettings.xaml
    /// </summary>
    public partial class EditNetworkSettings : Page
    {
        /// <summary>
        /// Идентификатор сетевых настроек (null для новых настроек)
        /// </summary>
        int? NetworkID = null;

        /// <summary>
        /// Событие удаления записи сетевых настроек
        /// </summary>
        public event EventHandler RecordDelete;

        /// <summary>
        /// Событие успешного сохранения записи
        /// </summary>
        public event EventHandler RecordSuccess;

        /// <summary>
        /// Конструктор страницы редактирования сетевых настроек
        /// </summary>
        /// <param name="NetworkSetting">Редактируемые сетевые настройки (null для создания новых)</param>
        public EditNetworkSettings(NetworkSettings NetworkSetting = null)
        {
            InitializeComponent();
            List<Equipment> equipment = new List<Equipment>();
            using var databaseContext = new DatabaseContext();
            try
            {
                equipment = databaseContext.Equipment.ToList();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            equipment.Insert(0, new Equipment { EquipmentID = -1, Name = "Отсутствует" });

            Equipment.ItemsSource = equipment;
            Equipment.DisplayMemberPath = "Name";
            Equipment.SelectedValuePath = "EquipmentID";

            if (NetworkSetting != null)
            {
                Equipment.SelectedValue = NetworkSetting.EquipmentID ?? -1;
                NetworkID = NetworkSetting.NetworkID;
                IPAddress.Text = NetworkSetting.IPAddress;
                SubnetMask.Text = NetworkSetting.SubnetMask;
                Gateway.Text = NetworkSetting.Gateway;
                DNSServers.Text = NetworkSetting.DNSServers;
            }
        }

        /// <summary>
        /// Проверяет корректность заполнения всех полей формы
        /// </summary>
        /// <returns>True если есть ошибки валидации, иначе False</returns>
        private bool ValidateAllFields()
        {
            bool incorrect = false;

            // Шаблон для проверки формата IP-адреса
            string ipRegex = @"^[0-9]{1,3}(\.[0-9]{1,3}){3}$";

            // Проверка IPAddress: обязательное поле, максимальная длина 15 символов.
            incorrect |= UIHelper.ValidateField(IPAddress.Text, 15, "IP Address", ipRegex, isRequired: true);

            // Проверка SubnetMask: необязательное поле, но если заполнено, должно соответствовать формату.
            incorrect |= UIHelper.ValidateField(SubnetMask.Text, 15, "Subnet Mask", ipRegex, isRequired: false);

            // Проверка Gateway: необязательное поле, но если заполнено, должно соответствовать формату.
            incorrect |= UIHelper.ValidateField(Gateway.Text, 15, "Gateway", ipRegex, isRequired: false);

            // Проверка DNSServers: если поле не пустое, предполагается, что адреса разделены запятыми.
            if (!string.IsNullOrWhiteSpace(DNSServers.Text))
            {
                var servers = DNSServers.Text.Split(',');
                foreach (var server in servers)
                {
                    // Обрезаем пробелы и проверяем каждый IP-адрес
                    incorrect |= UIHelper.ValidateField(server.Trim(), 15, "DNS Server", ipRegex, isRequired: true);
                }
            }

            return incorrect;
        }

        /// <summary>
        /// Обновляет объект сетевых настроек значениями из элементов управления
        /// </summary>
        /// <param name="networkSettingToUpdate">Объект для обновления</param>
        /// <param name="databaseContext">Контекст базы данных</param>
        private void UpdatesFromControls(NetworkSettings networkSettingToUpdate, DatabaseContext databaseContext)
        {
            networkSettingToUpdate.IPAddress = IPAddress.Text;
            networkSettingToUpdate.SubnetMask = SubnetMask.Text;

            var equipment = Equipment.SelectedItem as Equipment;
            if (equipment != null && equipment.EquipmentID != -1)
            {
                networkSettingToUpdate.EquipmentID = equipment.EquipmentID;
                databaseContext.Equipment.Attach(equipment);
                networkSettingToUpdate.Equipment = equipment;
            }
            else
            {
                networkSettingToUpdate.EquipmentID = null;
                networkSettingToUpdate.Equipment = null;
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки сохранения изменений
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Параметры события</param>
        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
                return;

            using var databaseContext = new DatabaseContext();
            try
            {
                if (databaseContext.NetworkSettings.Any(x => x.IPAddress == IPAddress.Text))
                {
                    MessageBox.Show("IP адресы не могут совпадать");
                    return;
                }

                var networkSettingFromDb = NetworkID.HasValue
                    ? databaseContext.NetworkSettings.FirstOrDefault(a => a.NetworkID == NetworkID.Value)
                    : null;

                if (networkSettingFromDb == null && NetworkID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                networkSettingFromDb ??= new NetworkSettings();

                UpdatesFromControls(networkSettingFromDb, databaseContext);

                if (!NetworkID.HasValue)
                {
                    databaseContext.NetworkSettings.Add(networkSettingFromDb);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(networkSettingFromDb, EventArgs.Empty);

                MainWindow.mainFrame.GoBack();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки отмены изменений
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Параметры события</param>
        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }
    }
}