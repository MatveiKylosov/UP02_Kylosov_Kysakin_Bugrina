using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP02.Context;
using UP02.Database;
using UP02.Elements;
using UP02.Helpers;
using UP02.Models;
using UP02.Pages.Elements;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Логика взаимодействия для PageInventories.xaml
    /// </summary>
    public partial class PageInventories : Page
    {
        List<Inventories> OriginalRecords = new List<Inventories>();
        List<Inventories> CurrentList = new List<Inventories>();

        /// <summary>
        /// Конструктор страницы, инициализирует компоненты, загружает данные об инвентаре и пользователях из базы данных.
        /// Проводится настройка видимости кнопки добавления записи в зависимости от роли пользователя.
        /// </summary>
        public PageInventories()
        {
            InitializeComponent();
            if(Settings.CurrentUser.Role != "Администратор")
            {
                AddNewRecordButton.Visibility= Visibility.Hidden;
            }

                using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.Inventories.Include(a => a.User).ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
            var users = OriginalRecords.Where(u => u != null && u.User != null).Select(e => e.User).Distinct().ToList();
            users.Insert(0, new Users { UserID = -1, LastName = "", FirstName = "Отсутствует", MiddleName = "" });

            UsersCB.ItemsSource = users;
            UsersCB.DisplayMemberPath = "FullName";
            UsersCB.SelectedValuePath = "UserID";

            UsersCB.SelectedValue = -1;

            CurrentList = OriginalRecords;
            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemInventories(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке "Добавить новую запись". Открывает страницу для редактирования инвентаря.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditInventories();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания новой записи инвентаря. Добавляет запись в список и выполняет сортировку.
        /// </summary>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var inventories = sender as Inventories;
            if (inventories == null)
                return;

            OriginalRecords.Add(inventories);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var inventories = sender as Inventories;
            if (inventories == null)
                return;

            var inventoriesToUpdate = OriginalRecords.Find(x => x.InventoryID == inventories.InventoryID);
            if (inventoriesToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(inventoriesToUpdate);
                OriginalRecords[index] = inventories;
            }
            SortRecord();
        }


        /// <summary>
        /// Выполняет сортировку и фильтрацию списка инвентаря по выбранному пользователю, дате и поисковому запросу.
        /// </summary>
        private void SortRecord()
        {
            CurrentList = OriginalRecords;

            int? selectedUser = UsersCB.SelectedValue as int?;
            if (selectedUser.HasValue && selectedUser.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.UserID == selectedUser.Value).ToList();
            }

            string searchQuery = SearchField.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                CurrentList = CurrentList
                    .Where(x => x.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                (x.StartDateString != null && x.StartDateString.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (x.User != null && x.User.FullName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (x.EndDateString != null && x.EndDateString.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0)
                          )
                    .ToList();
            }

            if (StartDate.SelectedDate.HasValue)
            {
                DateTime startDate = StartDate.SelectedDate.Value.Date; // Берем только дату без времени
                CurrentList = CurrentList.Where(x => x.StartDate.Date == startDate).ToList(); // Сравниваем только дату
            }

            if (EndDate.SelectedDate.HasValue)
            {
                DateTime endDate = EndDate.SelectedDate.Value.Date; // Берем только дату без времени
                CurrentList = CurrentList.Where(x => x.EndDate.Date == endDate).ToList(); // Сравниваем только дату
            }

            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemInventories(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик изменения выбора в комбобоксе для сортировки. Перезапускает сортировку записей.
        /// </summary>
        private void SortCB_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Поиск". Запускает процесс сортировки и фильтрации списка инвентаря.
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик изменения выбранной даты начала. Перезапускает сортировку записей.
        /// </summary>
        private void Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик ввода текста в поле даты. Перезапускает сортировку записей.
        /// </summary>
        private void Date_TextInput(object sender, TextCompositionEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик потери фокуса полем даты. Перезапускает сортировку записей.
        /// </summary>
        private void Date_LostFocus(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }
    }
}
