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
    /// Логика взаимодействия для PageEquipmentModels.xaml
    /// </summary>
    public partial class PageEquipmentModels : Page
    {
        List<EquipmentModels> OriginalRecords = new List<EquipmentModels>();
        List<EquipmentModels> CurrentList = new List<EquipmentModels>();

        /// <summary>
        /// Конструктор страницы, инициализирует компоненты и загружает данные о моделях оборудования из базы данных.
        /// </summary>
        public PageEquipmentModels()
        {
            InitializeComponent();
                using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.EquipmentModels.ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
            CurrentList = OriginalRecords;
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemEquipmentModels(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке "Добавить новую запись". Открывает страницу для редактирования модели оборудования.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditEquipmentModels();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания новой записи модели оборудования. Добавляет модель в список и выполняет сортировку.
        /// </summary>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var equipmentModels = sender as EquipmentModels;
            if (equipmentModels == null)
                return;

            OriginalRecords.Add(equipmentModels);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var equipmentModels = sender as EquipmentModels;
            if (equipmentModels == null)
                return;
            
            var equipmentModelsToUpdate = OriginalRecords.Find(x => x.ModelID == equipmentModels.ModelID);
            if (equipmentModelsToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(equipmentModelsToUpdate);
                if(index != -1)
                    OriginalRecords[index] = equipmentModels;
            }
            SortRecord();
        }
        /// <summary>
        /// Выполняет сортировку и фильтрацию списка моделей оборудования по введенному поисковому запросу.
        /// </summary>
        private void SortRecord()
        {
            CurrentList = OriginalRecords;

            string searchQuery = SearchField.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                CurrentList = CurrentList
                    .Where(x => x.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .ToList();
            }

            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemEquipmentModels(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке "Поиск". Запускает процесс сортировки и фильтрации списка моделей оборудования.
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }
    }
}
