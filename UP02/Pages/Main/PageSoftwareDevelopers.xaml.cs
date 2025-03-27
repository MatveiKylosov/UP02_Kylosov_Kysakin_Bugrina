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
    /// Логика взаимодействия для PageSoftwareDevelopers.xaml
    /// </summary>
    public partial class PageSoftwareDevelopers : Page
    {
        List<SoftwareDevelopers> OriginalRecords = new List<SoftwareDevelopers>();
        List<SoftwareDevelopers> CurrentList = new List<SoftwareDevelopers>();

        /// <summary>
        /// Конструктор страницы разработчиков программного обеспечения. Инициализирует компоненты и загружает данные о разработчиках 
        /// программного обеспечения из базы данных.
        /// </summary>
        public PageSoftwareDevelopers()
        {
            InitializeComponent();
                using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.SoftwareDevelopers.ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
            CurrentList = OriginalRecords;
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemSoftwareDevelopers(x), OriginalRecords, UpdateRecordSuccess);
        }


        /// <summary>
        /// Обработчик клика по кнопке "Добавить новую запись". Открывает страницу для редактирования новой записи разработчика программного обеспечения.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditSoftwareDevelopers();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания новой записи разработчика программного обеспечения. Добавляет запись в оригинальный список и сортирует записи.
        /// </summary>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var SoftwareDevelopers = sender as SoftwareDevelopers;
            if (SoftwareDevelopers == null)
                return;

            OriginalRecords.Add(SoftwareDevelopers);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var SoftwareDevelopers = sender as SoftwareDevelopers;
            if (SoftwareDevelopers == null)
                return;

            var SoftwareDevelopersToUpdate = OriginalRecords.Find(x => x.DeveloperID == SoftwareDevelopers.DeveloperID);
            if (SoftwareDevelopersToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(SoftwareDevelopersToUpdate);
                OriginalRecords[index] = SoftwareDevelopers;
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
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemSoftwareDevelopers(x), OriginalRecords, UpdateRecordSuccess);
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
