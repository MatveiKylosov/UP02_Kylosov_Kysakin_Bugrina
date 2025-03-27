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
    /// Логика взаимодействия для PageUsers.xaml
    /// </summary>
    public partial class PageUsers : Page
    {
        List<Users> OriginalRecords = new List<Users>();
        List<Users> CurrentList = new List<Users>();

        /// <summary>
        /// Конструктор страницы пользователей. Инициализирует компоненты и загружает данные из базы данных.
        /// </summary>
        public PageUsers()
        {
            InitializeComponent();
                using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.Users.ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
            CurrentList = OriginalRecords;

            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemUsers(x), OriginalRecords, UpdateRecordSuccess);
        }


        /// <summary>
        /// Обработчик клика по кнопке "Добавить нового пользователя". Открывает страницу для редактирования нового пользователя.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditUsers();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания нового пользователя. Добавляет пользователя в оригинальный список и сортирует записи.
        /// </summary>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var Users = sender as Users;
            if (Users == null)
                return;

            OriginalRecords.Add(Users);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var Users = sender as Users;
            if (Users == null)
                return;

            var UsersToUpdate = OriginalRecords.Find(x => x.UserID == Users.UserID);
            if (UsersToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(UsersToUpdate);
                OriginalRecords[index] = Users;
            }
            SortRecord();
        }

        /// <summary>
        /// Метод сортировки и фильтрации пользователей. Применяет фильтры по поисковому запросу и роли пользователя, обновляя отображаемые записи.
        /// </summary>
        private void SortRecord()
        {
            if (ContentPanel == null)
                return;

            CurrentList = OriginalRecords;
            string selectedRole = (RoleCB.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selectedRole != null && selectedRole != "Отсутствует")
            {
                CurrentList = CurrentList.Where(x => x.Role == selectedRole).ToList();
            }

            string searchQuery = SearchField.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                CurrentList = CurrentList
                    .Where(x => x.FullName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                x.Role.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                (x.Email != null && x.Email.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (x.Phone != null && x.Phone.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (x.Address != null && x.Address.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0))
                    .ToList();
            }

            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemUsers(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик изменения выбора роли в ComboBox. Перезапускает сортировку с применением фильтра по выбранной роли.
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
