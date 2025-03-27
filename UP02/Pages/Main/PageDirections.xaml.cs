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
    /// Логика взаимодействия для PageDirections.xaml
    /// </summary>
    public partial class PageDirections : Page
    {
        List<Directions> OriginalRecords = new List<Directions>();
        List<Directions> CurrentList = new List<Directions>();

        /// <summary>
        /// Конструктор страницы, инициализирует компоненты и загружает данные из базы данных.
        /// </summary>
        public PageDirections()
        {
            InitializeComponent();
                using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.Directions.ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            UIHelper.AddItemsToPanel(ContentPanel, OriginalRecords, x => new ItemDirections(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке для добавления нового направления.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditDirections();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания нового направления, добавляет запись в список и сортирует данные.
        /// </summary>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var direction = sender as Directions;
            if (direction == null)
                return;

            OriginalRecords.Add(direction);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var directions = sender as Directions;
            if (directions == null)
                return;

            var directionsToUpdate = OriginalRecords.Find(x => x.DirectionID == directions.DirectionID);
            if (directionsToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(directionsToUpdate);
                OriginalRecords[index] = directions;
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
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemDirections(x), OriginalRecords, UpdateRecordSuccess);
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
