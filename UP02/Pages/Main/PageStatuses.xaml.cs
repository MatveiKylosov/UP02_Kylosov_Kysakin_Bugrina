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
    /// Логика взаимодействия для PageStatuses.xaml
    /// </summary>
    public partial class PageStatuses : Page
    {
        List<Statuses> OriginalRecords = new List<Statuses>();
        List<Statuses> CurrentList = new List<Statuses>();

        /// <summary>
        /// Конструктор страницы статусов. Инициализирует компоненты и загружает данные о статусах из базы данных.
        /// </summary>
        public PageStatuses()
        {
            InitializeComponent();
                using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.Statuses.ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            UIHelper.AddItemsToPanel(ContentPanel, OriginalRecords, x => new ItemStatuses(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Конструктор страницы статусов. Инициализирует компоненты и загружает данные о статусах из базы данных.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditStatuses();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания новой записи статуса. Добавляет запись в оригинальный список и сортирует записи.
        /// </summary>

        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var statuses = sender as Statuses;
            if (statuses == null)
                return;

            OriginalRecords.Add(statuses);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var statuses = sender as Statuses;
            if (statuses == null)
                return;

            var statusesToUpdate = OriginalRecords.Find(x => x.StatusID == statuses.StatusID);
            if (statusesToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(statusesToUpdate);
                OriginalRecords[index] = statuses;
            }
            SortRecord();
        }

        /// <summary>
        /// Метод сортировки записей. Применяет фильтры по поисковому запросу, обновляя отображаемые записи.
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
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemStatuses(x), OriginalRecords, UpdateRecordSuccess);
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
