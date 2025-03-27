using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
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
using UP02.Database;
using UP02.Helpers;
using UP02.Interfaces;
using UP02.Models;
using UP02.Pages.Main;

namespace UP02.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для EditUsers.xaml
    /// </summary>
    public partial class EditUsers : Page, IRecordSuccess
    {
        bool ChangeLoginOrPasswordOrRole = false;
        /// <summary>
        /// Идентификатор пользователя (null для нового пользователя)
        /// </summary>
        int? UserID = null;

        /// <summary>
        /// Событие удаления записи пользователя
        /// </summary>
        public event EventHandler RecordDelete;

        /// <summary>
        /// Событие успешного сохранения изменений
        /// </summary>
        public event EventHandler RecordSuccess;

        /// <summary>
        /// Конструктор страницы редактирования пользователя
        /// </summary>
        /// <param name="user">Редактируемый пользователь (null для создания нового)</param>
        public EditUsers(Users user = null)
        {
            InitializeComponent();
            if (user != null)
            {
                LastName.Text = user.LastName;
                FirstName.Text = user.FirstName;
                MiddleName.Text = user.MiddleName;
                Role.SelectedValue = user.Role;
                Login.Text = user.Login;
                Password.Password = user.Password;
                Email.Text = user.Email;
                Phone.Text = user.Phone;
                Address.Text = user.Address;
                UserID = user.UserID;
            }
            else

                Role.SelectedIndex = 1;
        }

        /// <summary>
        /// Проверяет корректность заполнения всех полей формы
        /// </summary>
        /// <returns>True если есть ошибки валидации, иначе False</returns>
        private bool ValidateAllFields()
        {
            bool incorrect = false;

            incorrect |= UIHelper.ValidateField(Login.Text, 50, "Логин", @"^[a-zA-Z0-9_]+$", isRequired: true);
            incorrect |= UIHelper.ValidateField(Password.Password, 255, "Пароль", @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d!@#$%^&*()_+\-=]{8,}$", isRequired: true);
            incorrect |= UIHelper.ValidateField(Email.Text, 255, "Почта", @"^[\w.-]+@[a-zA-Z\d.-]+\.[a-zA-Z]{2,6}$", isRequired: false);
            incorrect |= UIHelper.ValidateField(LastName.Text, 100, "Фамилия", @"^[А-Яа-яЁёA-Za-z\-]+$", isRequired: true);
            incorrect |= UIHelper.ValidateField(FirstName.Text, 100, "Имя", @"^[А-Яа-яЁёA-Za-z\-]+$", isRequired: true);
            incorrect |= UIHelper.ValidateField(MiddleName.Text, 100, "Отчество", @"^[А-Яа-яЁёA-Za-z\-]+$", isRequired: false);
            incorrect |= UIHelper.ValidateField(Phone.Text, 20, "Телефон", @"^\+7\d{10}$", isRequired: false);
            incorrect |= UIHelper.ValidateField(Address.Text, 255, "Адрес", isRequired: false);

            if (Role.SelectedIndex == -1)
            {
                incorrect = true;
                MessageBox.Show("У пользователя должна быть указана роль.", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return incorrect;
        }

        /// <summary>
        /// Обновляет данные пользователя из элементов управления
        /// </summary>
        /// <param name="userToUpdate">Объект пользователя для обновления</param>
        private void UpdatesFromControls(Users userToUpdate)
        {
            ChangeLoginOrPasswordOrRole = Login.Text != userToUpdate.Login || Password.Password != userToUpdate.Password || (Role.SelectedItem as ComboBoxItem)?.Content.ToString() != userToUpdate.Role;
            userToUpdate.LastName = LastName.Text;
            userToUpdate.FirstName = FirstName.Text;
            userToUpdate.MiddleName = MiddleName.Text;
            userToUpdate.Role = (Role.SelectedItem as ComboBoxItem)?.Content.ToString();
            userToUpdate.Login = Login.Text;
            userToUpdate.Password = Password.Password;
            userToUpdate.Email = Email.Text;
            userToUpdate.Phone = Phone.Text;
            userToUpdate.Address = Address.Text;
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки сохранения изменений
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
                return;
            using var databaseContext = new DatabaseContext();
            try
            {
                var userFromDb = UserID.HasValue
                    ? databaseContext.Users.FirstOrDefault(s => s.UserID == UserID.Value)
                    : null;

                if (userFromDb == null && UserID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                userFromDb ??= new Users();

                UpdatesFromControls(userFromDb);

                if(databaseContext.Users.Any(x => x.Login == userFromDb.Login && x.UserID != userFromDb.UserID))
                {
                    MessageBox.Show("У пользователя должен быть уникальный логин.", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!UserID.HasValue)
                {
                    databaseContext.Users.Add(userFromDb);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(userFromDb, EventArgs.Empty);

                if (userFromDb.UserID == Settings.CurrentUser.UserID && ChangeLoginOrPasswordOrRole) 
                {
                    MainWindow.ClearFrame();
                    MainWindow.OpenPage(new PageAuthorization());
                    MessageBox.Show("Необходимо авторизоваться.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MainWindow.GoBack();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки отмены изменений
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }
    }
}