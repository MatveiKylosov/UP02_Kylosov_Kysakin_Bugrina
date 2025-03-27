using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using UP02.Interfaces;
using UP02.Models;
using UP02.Pages.Elements;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Страница для просмотра и управления списком аудиторий
    /// </summary>
    public partial class PageAudiences : Page
    {
        /// <summary>
        /// Оригинальный список всех аудиторий
        /// </summary>
        List<Audiences> OriginalRecords = new List<Audiences>();

        /// <summary>
        /// Текущий отфильтрованный список аудиторий
        /// </summary>
        List<Audiences> CurrentList = new List<Audiences>();

        /// <summary>
        /// Инициализирует новый экземпляр страницы аудиторий и загружает данные
        /// </summary>
        public PageAudiences()
        {
            InitializeComponent();
            using var databaseContext = new DatabaseContext();
            List<Users> users;

            try
            {
                OriginalRecords = databaseContext.Audiences.Include(a => a.ResponsibleUser)
                .Include(a => a.TempResponsibleUser)
                .ToList();

            }
            catch (Exception ex)
            {
                bool WrongConnections = false;
                try
                {
                    var errorEntry = new Errors
                    {
                        ErrorTime = DateTime.Now,
                        ErrorMessage = ex.Message,
                    };
                    databaseContext.Errors.Add(errorEntry);
                    databaseContext.SaveChanges();
                }
                catch
                {
                    WrongConnections = true;
                }

                MessageBox.Show(WrongConnections ? "Не удалось подключиться к базе данных. Проверьте соединение и повторите попытку." : "произошла непредвиденная ошибка.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                if (WrongConnections)
                {
                    MainWindow.OpenPage(new PageAuthorization());
                }
                return;
            }

            users = OriginalRecords
               .SelectMany(a => new[] { a.ResponsibleUser, a.TempResponsibleUser })
               .Where(u => u != null)
               .Distinct()
               .ToList();

            users.Insert(0, new Users { UserID = -1, LastName = "", FirstName = "Отсутствует", MiddleName = "" });

            ResponsibleUserCB.ItemsSource = users;
            TempResponsibleUserCB.ItemsSource = users;

            TempResponsibleUserCB.DisplayMemberPath = ResponsibleUserCB.DisplayMemberPath = "FullName";
            TempResponsibleUserCB.SelectedValuePath = ResponsibleUserCB.SelectedValuePath = "UserID";

            TempResponsibleUserCB.SelectedValue = -1;
            ResponsibleUserCB.SelectedValue = -1;

            CurrentList = OriginalRecords;

            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemAudiences(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки добавления новой аудитории
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Аргументы события</param>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditAudiences();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обрабатывает успешное создание новой аудитории
        /// </summary>
        /// <param name="sender">Созданная аудитория</param>
        /// <param name="e">Аргументы события</param>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var audience = sender as Audiences;
            if (audience == null)
                return;

            OriginalRecords.Add(audience);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var Audiences = sender as Audiences;
            if (Audiences == null)
                return;

            var AudiencesToUpdate = OriginalRecords.Find(x => x.AudienceID == Audiences.AudienceID);
            if (AudiencesToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(AudiencesToUpdate);
                OriginalRecords[index] = Audiences;
            }
            SortRecord();
        }

        /// <summary>
        /// Выполняет сортировку и фильтрацию списка аудиторий
        /// </summary>
        private void SortRecord()
        {
            CurrentList = OriginalRecords;

            int? selectedResponsible = ResponsibleUserCB.SelectedValue as int?;
            if (selectedResponsible.HasValue && selectedResponsible.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.ResponsibleUserID == selectedResponsible.Value).ToList();
            }

            int? selectedTempResponsible = TempResponsibleUserCB.SelectedValue as int?;
            if (selectedTempResponsible.HasValue && selectedTempResponsible.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.TempResponsibleUserID == selectedTempResponsible.Value).ToList();
            }

            string searchQuery = SearchField.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                CurrentList = CurrentList
                    .Where(x => x.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                (x.ShortName != null && x.ShortName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (x.ResponsibleUser != null && x.ResponsibleUser.FullName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (x.TempResponsibleUser != null && x.TempResponsibleUser.FullName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0))
                    .ToList();
            }

            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemAudiences(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обрабатывает изменение выбора в комбобоксах фильтрации
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Аргументы события</param>
        private void SortCB_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки поиска
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие</param>
        /// <param name="e">Аргументы события</param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }
    }
}