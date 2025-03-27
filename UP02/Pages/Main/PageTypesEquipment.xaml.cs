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
    /// Логика взаимодействия для PageTypesEquipment.xaml
    /// </summary>
    public partial class PageTypesEquipment : Page
    {
        List<TypesEquipment> OriginalRecords = new List<TypesEquipment>();
        List<TypesEquipment> CurrentList = new List<TypesEquipment>();

        public PageTypesEquipment()
        {
            InitializeComponent();

                using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.TypesEquipment.ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }

            UIHelper.AddItemsToPanel(ContentPanel, OriginalRecords, x => new ItemTypesEquipment(x), OriginalRecords, UpdateRecordSuccess);
        }
        /// <summary>
        /// Обработчик клика по кнопке для добавления нового направления.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditTypesEquipment();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания нового направления, добавляет запись в список и сортирует данные.
        /// </summary>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var TypesEquipment = sender as TypesEquipment;
            if (TypesEquipment == null)
                return;

            OriginalRecords.Add(TypesEquipment);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var TypesEquipment = sender as TypesEquipment;
            if (TypesEquipment == null)
                return;

            var TypesEquipmentToUpdate = OriginalRecords.Find(x => x.TypeEquipmentID == TypesEquipment.TypeEquipmentID);
            if (TypesEquipmentToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(TypesEquipmentToUpdate);
                OriginalRecords[index] = TypesEquipment;
            }
            SortRecord();
        }

        /// <summary>
        /// Метод для сортировки и фильтрации записей на основе поискового запроса.
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
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemTypesEquipment(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке поиска, выполняет сортировку данных.
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }
    }
}
