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
using UP02.Helpers;
using UP02.Models;
using UP02.Interfaces;
using UP02.Pages.Main;

namespace UP02.Pages.Elements
{
    /// <summary>
    /// Страница редактирования информации об аудитории
    /// </summary>
    public partial class EditAudiences : Page, IRecordSuccess
    {
        /// <summary>
        /// Идентификатор аудитории. Null если создается новая аудитория.
        /// </summary>
        int? AudienceID = null;

        /// <summary>
        /// Событие удаления записи
        /// </summary>
        public event EventHandler RecordDelete;

        /// <summary>
        /// Событие успешного сохранения записи
        /// </summary>
        public event EventHandler RecordSuccess;

        /// <summary>
        /// Конструктор страницы редактирования аудитории
        /// </summary>
        /// <param name="audience">Аудитория для редактирования. Если null - создается новая</param>
        public EditAudiences(Audiences audience = null)
        {
            InitializeComponent();

            List<Users> _Users = new List<Users>();
            using var databaseContext = new DatabaseContext();
            try
            {
                _Users = databaseContext.Users.ToList();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }

            _Users.Insert(0, new Users { UserID = -1, LastName = "", FirstName = "Отсутствует", MiddleName = "" });

            ResponsibleUserCB.ItemsSource = _Users;
            TempResponsibleUserCB.ItemsSource = _Users;

            TempResponsibleUserCB.DisplayMemberPath = ResponsibleUserCB.DisplayMemberPath = "FullName";
            TempResponsibleUserCB.SelectedValuePath = ResponsibleUserCB.SelectedValuePath = "UserID";

            if (audience != null)
            {
                Name.Text = audience.Name;
                ShortName.Text = audience.ShortName;
                AudienceID = audience.AudienceID;
            }
            ResponsibleUserCB.SelectedValue = audience?.ResponsibleUserID ?? -1;
            TempResponsibleUserCB.SelectedValue = audience?.TempResponsibleUserID ?? -1;
        }

        /// <summary>
        /// Проверяет корректность заполнения всех полей формы
        /// </summary>
        /// <returns>True если есть ошибки валидации, иначе False</returns>
        private bool ValidateAllFields()
        {
            bool incorrect = false;

            incorrect |= UIHelper.ValidateField(Name.Text, 255, "Название", isRequired: true);
            incorrect |= UIHelper.ValidateField(ShortName.Text, 50, "Краткое название", isRequired: false);

            int? responsibleUserSelect = ResponsibleUserCB.SelectedValue as int?;
            int? tempResponsibleUserSelect = TempResponsibleUserCB.SelectedValue as int?;
            if ((!responsibleUserSelect.HasValue || responsibleUserSelect.Value == -1) && (!tempResponsibleUserSelect.HasValue || tempResponsibleUserSelect == -1))
            {
                incorrect = true;
                MessageBox.Show("Не выбран ответственный пользователь.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return incorrect;
        }

        /// <summary>
        /// Обновляет объект аудитории значениями из элементов управления
        /// </summary>
        /// <param name="audienceToUpdate">Аудитория для обновления</param>
        /// <param name="databaseContext">Контекст базы данных</param>
        private void UpdatesFromControls(Audiences audienceToUpdate, DatabaseContext databaseContext)
        {
            audienceToUpdate.Name = Name.Text;
            audienceToUpdate.ShortName = ShortName.Text;

            UIHelper.SetEntity<Users, int?>(
                ResponsibleUserCB,
                id => audienceToUpdate.ResponsibleUserID = id,
                entity => audienceToUpdate.ResponsibleUser = entity,
                u => u.UserID,
                -1,
                databaseContext.Users);

            UIHelper.SetEntity<Users, int?>(
                TempResponsibleUserCB,
                id => audienceToUpdate.TempResponsibleUserID = id,
                entity => audienceToUpdate.TempResponsibleUser = entity,
                u => u.UserID,
                -1,
                databaseContext.Users);
        }

        /// <summary>
        /// Обработчик нажатия кнопки сохранения изменений
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
                return;

            Audiences? audienceFromdatabaseContext;
            using var databaseContext = new DatabaseContext();
            try
            {
                audienceFromdatabaseContext = AudienceID.HasValue
                    ? databaseContext.Audiences.FirstOrDefault(a => a.AudienceID == AudienceID.Value)
                    : null;
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }

            if (audienceFromdatabaseContext == null && AudienceID.HasValue)
            {
                MessageBox.Show(
                    "Запись в базе данных не найдена.",
                    "Запись не найдена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                RecordDelete?.Invoke(null, EventArgs.Empty);
                return;
            }

            audienceFromdatabaseContext ??= new Audiences();

            try
            {
                UpdatesFromControls(audienceFromdatabaseContext, databaseContext);
                if (!AudienceID.HasValue)
                {
                    databaseContext.Audiences.Add(audienceFromdatabaseContext);
                }

                databaseContext.SaveChanges();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }

            RecordSuccess?.Invoke(audienceFromdatabaseContext, EventArgs.Empty);

            MainWindow.mainFrame.GoBack();
        }

        /// <summary>
        /// Обработчик нажатия кнопки отмены изменений
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }
    }
}