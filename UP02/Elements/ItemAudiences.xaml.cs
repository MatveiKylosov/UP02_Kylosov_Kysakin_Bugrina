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
    /// Логика взаимодействия для ItemAudiences.xaml
    /// </summary>
    public partial class ItemAudiences : UserControl, IRecordDeletable, IRecordUpdatable
    {
        /// <summary>
        /// Событие, возникающее при удалении записи.
        /// </summary>
        public event EventHandler RecordDelete;        public event EventHandler RecordUpdate;

        /// <summary>
        /// Объект аудитории, связанный с данным элементом.
        /// </summary>
        private Audiences Audience;

        /// <summary>
        /// Создает новый экземпляр <see cref="ItemAudiences"/>.
        /// </summary>
        /// <param name="Audience">Объект аудитории для привязки данных.</param>
        public ItemAudiences(Audiences Audience)
        {
            InitializeComponent();
            this.Audience = Audience;
            this.DataContext = Audience;
        }

        /// <summary>
        /// Обработчик события удаления аудитории.
        /// Проверяет наличие связей в БД перед удалением.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                if (databaseContext.Equipment.Any(e => e.AudienceID == Audience.AudienceID))
                {
                    MessageBox.Show("Нельзя удалить, есть связи");
                    return;
                }

                var audience = databaseContext.Audiences.FirstOrDefault(x => x.AudienceID == Audience.AudienceID);
                if (audience != null)
                {
                    databaseContext.Audiences.Remove(audience);
                    databaseContext.SaveChanges();
                }

                RecordDelete?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
        }


        /// <summary>
        /// Обработчик события обновления аудитории.
        /// Открывает страницу редактирования аудитории.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new Pages.Elements.EditAudiences(Audience);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Вызывается после успешного редактирования аудитории.
        /// Обновляет локальные данные.
        /// </summary>
        /// <param name="sender">Объект, содержащий обновленные данные аудитории.</param>
        /// <param name="e">Данные события.</param>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.Audience = sender as Audiences;
            this.DataContext = this.Audience;
            RecordUpdate?.Invoke(Audience, EventArgs.Empty);
        }

        /// <summary>
        /// Обработчик удаления записи из редактора.
        /// Генерирует событие удаления.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Данные события.</param>
        private void _RecordDelete(object sender, EventArgs e)
        {
            RecordDelete?.Invoke(this, e);
        }
    }
}
