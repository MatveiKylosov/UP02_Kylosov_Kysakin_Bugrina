using Microsoft.EntityFrameworkCore;
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
using UP02.Elements;
using UP02.Helpers;
using UP02.Models;
using UP02.Pages.Elements;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Логика взаимодействия для PageNetworkSettings.xaml
    /// </summary>
    public partial class PageNetworkSettings : Page
    {
        List<NetworkSettings> OriginalRecords = new List<NetworkSettings>();
        List<NetworkSettings> CurrentList = new List<NetworkSettings>();

        /// <summary>
        /// Конструктор страницы, инициализирует компоненты и загружает данные о настройках сети из базы данных.
        /// Настраивает комбобокс с оборудованием для фильтрации записей.
        /// </summary>
        public PageNetworkSettings()
        {
            InitializeComponent();
                using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.NetworkSettings
                                            .Include(a => a.Equipment)
                                            .ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            List<Equipment> equipment = OriginalRecords.Where(u => u != null).Select(a => a.Equipment).Distinct().ToList();
            equipment.Insert(0, new Equipment { Name = "Отсутствует", EquipmentID = -1 });

            EquipmentsCB.ItemsSource = equipment;
            EquipmentsCB.DisplayMemberPath = "Name";
            EquipmentsCB.SelectedValuePath = "EquipmentID";
            EquipmentsCB.SelectedValue = -1;

            CurrentList = OriginalRecords;

            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemNetworkSettings(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке "Добавить новую запись". Открывает страницу для редактирования настроек сети.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditNetworkSettings();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }


        /// <summary>
        /// Обработчик успешного создания новой записи настроек сети. Добавляет запись в список и выполняет сортировку.
        /// </summary>

        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var NetworkSettings = sender as NetworkSettings;
            if (NetworkSettings == null)
                return;

            OriginalRecords.Add(NetworkSettings);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var NetworkSettings = sender as NetworkSettings;
            if (NetworkSettings == null)
                return;

            var NetworkSettingsToUpdate = OriginalRecords.Find(x => x.NetworkID == NetworkSettings.NetworkID);
            if (NetworkSettingsToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(NetworkSettingsToUpdate);
                OriginalRecords[index] = NetworkSettings;
            }
            SortRecord();
        }

        /// <summary>
        /// Выполняет сортировку и фильтрацию списка настроек сети по выбранному оборудованию и поисковому запросу.
        /// </summary>
        private void SortRecord()
        {
            CurrentList = OriginalRecords;

            int? selectedEquipment = EquipmentsCB.SelectedValue as int?;
            if (selectedEquipment.HasValue && selectedEquipment.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.EquipmentID == selectedEquipment.Value).ToList();
            }

            string searchQuery = SearchField.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                CurrentList = CurrentList
                    .Where(x => x.IPAddress.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                x.SubnetMask.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                x.Gateway.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                x.DNSServers.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                (x.Equipment != null && x.Equipment.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0))
                    .ToList();
            }

            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemNetworkSettings(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик изменения выбора в комбобоксе для сортировки. Перезапускает сортировку записей.
        /// </summary>
        private void SortCB_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Поиск". Запускает процесс сортировки и фильтрации списка настроек сети.
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }

    }
}
