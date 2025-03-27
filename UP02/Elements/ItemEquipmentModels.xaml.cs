using System.Windows;
using System.Windows.Controls;
using UP02.Context;
using UP02.Helpers;
using UP02.Interfaces;
using UP02.Models;
using UP02.Pages.Elements;

namespace UP02.Elements
{
    /// <summary>
    /// Логика взаимодействия для ItemEquipmentModels.xaml
    /// </summary>
    public partial class ItemEquipmentModels : UserControl, IRecordDeletable
    {    /// <summary>
         /// Создает новый экземпляр и инициализирует его.
         /// </summary>
         /// <param name="EquipmentModel">Модель оборудования, которая будет отображаться.</param>
        public ItemEquipmentModels(EquipmentModels EquipmentModel)
        {
            InitializeComponent();
            this.EquipmentModel = EquipmentModel;
            this.DataContext = EquipmentModel;
        }

        /// <summary>
        /// Событие, возникающее при удалении записи.
        /// </summary>
        public event EventHandler RecordDelete;
        EquipmentModels EquipmentModel;

        /// <summary>
        /// Обработчик клика по кнопке удаления модели оборудования.
        /// </summary>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                if (databaseContext.Equipment.Any(e => e.ModelID == EquipmentModel.ModelID))
                {
                    MessageBox.Show("Нельзя удалить, есть связи");
                    return;
                }

                var equipmentModel = databaseContext.EquipmentModels.FirstOrDefault(em => em.ModelID == EquipmentModel.ModelID);
                if (equipmentModel != null)
                {
                    databaseContext.EquipmentModels.Remove(equipmentModel);
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
        /// Обработчик клика по кнопке обновления модели оборудования.
        /// </summary>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new EditEquipmentModels(EquipmentModel);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного редактирования модели оборудования.
        /// </summary>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.EquipmentModel = sender as EquipmentModels;
            this.DataContext = this.EquipmentModel;
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
