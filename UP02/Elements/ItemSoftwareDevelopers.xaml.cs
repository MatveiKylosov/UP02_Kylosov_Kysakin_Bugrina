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
    /// Логика взаимодействия для ItemSoftwareDevelopers.xaml
    /// </summary>
    public partial class ItemSoftwareDevelopers : UserControl, IRecordDeletable, IRecordUpdatable
    {
        public event EventHandler RecordDelete;        public event EventHandler RecordUpdate;
        private SoftwareDevelopers SoftwareDeveloper;

        /// <summary>
        /// Инициализирует новый экземпляр
        public ItemSoftwareDevelopers(SoftwareDevelopers SoftwareDeveloper)
        {
            InitializeComponent();
            this.SoftwareDeveloper = SoftwareDeveloper;
            this.DataContext = SoftwareDeveloper;
        }

        /// <summary>
        /// Обработчик клика по кнопке удаления разработчика программного обеспечения.
        /// </summary>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                if (databaseContext.Software.Any(s => s.DeveloperID == SoftwareDeveloper.DeveloperID))
                {
                    MessageBox.Show("Нельзя удалить, есть связи");
                    return;
                }

                var softwareDeveloper = databaseContext.SoftwareDevelopers.FirstOrDefault(sd => sd.DeveloperID == SoftwareDeveloper.DeveloperID);
                if (softwareDeveloper != null)
                {
                    databaseContext.SoftwareDevelopers.Remove(softwareDeveloper);
                    databaseContext.SaveChanges();
                }

                // Сигнализируем об удалении записи
                RecordDelete?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке обновления разработчика программного обеспечения.
        /// </summary>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new EditSoftwareDevelopers(SoftwareDeveloper);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик события успешного редактирования разработчика программного обеспечения.
        /// </summary>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.SoftwareDeveloper = sender as SoftwareDevelopers;
            this.DataContext = this.SoftwareDeveloper;
            RecordUpdate?.Invoke(SoftwareDeveloper, EventArgs.Empty);
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
