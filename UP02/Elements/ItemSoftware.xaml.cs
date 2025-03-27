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
    /// Логика взаимодействия для ItemSoftware.xaml
    /// </summary>
    public partial class ItemSoftware : UserControl, IRecordDeletable
    {
        public event EventHandler RecordDelete;
        private Software Software;

        /// <summary>
        /// Инициализирует новый экземпляр
        /// </summary>
        public ItemSoftware(Software Software)
        {
            InitializeComponent();
            this.DataContext = Software;
            this.Software = Software;
        }

        /// <summary>
        /// Обработчик клика по кнопке удаления программного обеспечения.
        /// </summary>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                var software = databaseContext.Software.FirstOrDefault(x => x.SoftwareID == Software.SoftwareID);
                if (software != null)
                {
                    databaseContext.Software.Remove(software);
                    databaseContext.SaveChanges();
                }

                RecordDelete?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке обновления программного обеспечения.
        /// </summary>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new Pages.Elements.EditSoftware(Software);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик события успешного редактирования программного обеспечения.
        /// </summary>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.Software = sender as Software;
            this.DataContext = this.Software;
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
