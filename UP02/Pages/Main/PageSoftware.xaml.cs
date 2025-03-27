using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using UP02.Context;
using UP02.Elements;
using UP02.Helpers;
using UP02.Models;
using UP02.Pages.Elements;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Логика взаимодействия для PageSoftware.xaml
    /// </summary>
    public partial class PageSoftware : Page
    {
        List<Software> OriginalRecords = new List<Software>();
        List<Software> CurrentList = new List<Software>();

        /// <summary>
        /// Конструктор страницы программного обеспечения. Инициализирует компоненты и загружает данные о программном обеспечении, 
        /// разработчиках и оборудовании из базы данных, устанавливает источники для комбинированных списков разработчиков и оборудования.
        /// </summary>
        public PageSoftware()
        {
            InitializeComponent();
                using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.Software.Include(a => a.Developer)
                                            .Include(a => a.Equipment)
                                            .ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }

            List<SoftwareDevelopers> softwareDevelopers = OriginalRecords.Where(e => e.Developer != null).Select(a => a.Developer).Distinct().ToList();
            softwareDevelopers.Insert(0, new SoftwareDevelopers { Name = "Отсутствует", DeveloperID = -1 });

            DevelopersCB.ItemsSource = softwareDevelopers;
            DevelopersCB.DisplayMemberPath = "Name";
            DevelopersCB.SelectedValuePath = "DeveloperID";
            DevelopersCB.SelectedValue = -1;

            List<Equipment> equipment = OriginalRecords.Where(u => u != null).Select(a => a.Equipment).Distinct().ToList();
            equipment.Insert(0, new Equipment { Name = "Отсутствует", EquipmentID = -1 });

            EquipmentsCB.ItemsSource = equipment;
            EquipmentsCB.DisplayMemberPath = "Name";
            EquipmentsCB.SelectedValuePath = "EquipmentID";
            EquipmentsCB.SelectedValue = -1;

            CurrentList = OriginalRecords;

            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemSoftware(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке "Добавить новую запись". Открывает страницу для редактирования новой записи программного обеспечения.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditSoftware();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания новой записи программного обеспечения. Добавляет запись в оригинальный список и сортирует записи.
        /// </summary>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var software = sender as Software;
            if (software == null)
                return;

            OriginalRecords.Add(software);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var software = sender as Software;
            if (software == null)
                return;

            var softwareToUpdate = OriginalRecords.Find(x => x.SoftwareID == software.SoftwareID);
            if (softwareToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(softwareToUpdate);
                OriginalRecords[index] = software;
            }
            SortRecord();
        }


        /// <summary>
        /// Метод сортировки записей. Применяет фильтры по разработчику, оборудованию и поисковому запросу, обновляя отображаемые записи.
        /// </summary>
        private void SortRecord()
        {
            CurrentList = OriginalRecords;

            int? selectedDeveloper = DevelopersCB.SelectedValue as int?;
            if (selectedDeveloper.HasValue && selectedDeveloper.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.DeveloperID == selectedDeveloper.Value).ToList();
            }

            int? selectedEquipment = EquipmentsCB.SelectedValue as int?;
            if (selectedEquipment.HasValue && selectedEquipment.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.EquipmentID == selectedEquipment.Value).ToList();
            }

            string searchQuery = SearchField.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                CurrentList = CurrentList
                    .Where(x => x.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                (x.Equipment != null && x.Equipment.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (x.Developer != null && x.Developer.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0))
                    .ToList();
            }

            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemSoftware(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик изменения значения в комбинированных списках сортировки (разработчик или оборудование). Перезапускает сортировку.
        /// </summary>
        private void SortCB_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Поиск". Перезапускает сортировку с применением фильтра по поисковому запросу.
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }
    }
}
