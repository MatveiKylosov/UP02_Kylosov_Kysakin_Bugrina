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
using UP02.Interfaces;
using UP02.Models;

namespace UP02.Elements
{
    /// <summary>
    /// Логика взаимодействия для ItemUsers.xaml
    /// </summary>
    public partial class ItemUsers : UserControl, IRecordDeletable
    {
        public event EventHandler RecordDelete;
        private Users User;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ItemUsers"/>.
        /// </summary>
        /// <param name="user">Пользователь для отображения.</param>
        public ItemUsers(Users user)
        {
            InitializeComponent();
            this.DataContext = user;
            this.User = user;
        }

        /// <summary>
        /// Обработчик клика по кнопке удаления пользователя.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                // Проверка наличия связей с другими объектами перед удалением пользователя
                var audience = databaseContext.Audiences.Any(a => a.TempResponsibleUserID == User.UserID || a.ResponsibleUserID == User.UserID);
                var equipment = databaseContext.Equipment.Any(e => e.TempResponsibleUserID == User.UserID || e.ResponsibleUserID == User.UserID);
                var consumable = databaseContext.Consumables.Any(x => x.TempResponsibleUserID == User.UserID);
                var inventory = databaseContext.Inventories.Any(x => x.UserID == User.UserID);

                // Если есть связи, пользователь не может быть удалён
                if (audience || equipment || consumable || inventory)
                {
                    MessageBox.Show("Удалить нельзя, имеется связи.");
                }
                else
                {
                    // Подтверждение удаления
                    var result = MessageBox.Show(
                        "Точно удалить?",
                        "Удаление",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    // Если пользователь подтвердил удаление
                    if (result == MessageBoxResult.Yes)
                    {
                        var delUser = databaseContext.Users.FirstOrDefault(u => u.UserID == User.UserID);
                        if (delUser != null)
                        {
                            databaseContext.Users.Remove(delUser);
                        }

                        databaseContext.SaveChanges();
                        RecordDelete?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при подключении
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке обновления пользователя.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new Pages.Elements.EditUsers(User);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик события успешного редактирования пользователя.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.User = sender as Users;
            this.DataContext = this.User;
        }

        /// <summary>
        /// Обработчик события удаления записи.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void _RecordDelete(object sender, EventArgs e)
        {
            RecordDelete?.Invoke(this, e);
        }
    }
}
