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
            try
            {
                using var databaseContext = new DatabaseContext();
                OriginalRecords = databaseContext.Statuses.ToList();
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте соединение и повторите попытку.",
                                "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);

                MainWindow.OpenPage(new PageAuthorization());
                return;
            }

            UIHelper.AddItemsToPanel(ContentPanel, OriginalRecords, x => new ItemStatuses(x), OriginalRecords);
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
            var direction = sender as Statuses;
            if (direction == null)
                return;

            OriginalRecords.Add(direction);
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
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemStatuses(x), OriginalRecords);
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
