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
using UP02.Pages.Elements;

namespace UP02.Elements
{
    /// <summary>
    /// Логика взаимодействия для ItemStatuses.xaml
    /// </summary>
    public partial class ItemStatuses : UserControl, IRecordDeletable, IRecordUpdatable
    {
        public event EventHandler RecordDelete;        public event EventHandler RecordUpdate;
        private Statuses Status;

        /// <summary>
        /// Инициализирует новый экземпляр
        /// </summary>
        public ItemStatuses(Statuses Status)
        {
            InitializeComponent();
            this.Status = Status;
            this.DataContext = Status;
        }

        /// <summary>
        /// Обработчик клика по кнопке удаления статуса.
        /// </summary>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                if (databaseContext.Equipment.Any(e => e.StatusID == Status.StatusID))
                {
                    MessageBox.Show("Нельзя удалить, есть связи");
                    return;
                }

                var status = databaseContext.Statuses.FirstOrDefault(s => s.StatusID == Status.StatusID);
                if (status != null)
                {
                    databaseContext.Statuses.Remove(status);
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
        /// Обработчик клика по кнопке обновления статуса.
        /// </summary>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new EditStatuses(Status);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик события успешного редактирования статуса.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.Status = sender as Statuses;
            this.DataContext = this.Status;
            RecordUpdate?.Invoke(Status, EventArgs.Empty);
        }

        /// <summary>
        /// Обработчик события удаления записи.
        /// </summary>
        private void _RecordDelete(object sender, EventArgs e)
        {
            RecordDelete?.Invoke(this, e);
        }
    }
}
