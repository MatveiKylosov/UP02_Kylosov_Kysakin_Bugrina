using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using UP02.Context;
using UP02.Helpers;
using UP02.Interfaces;
using UP02.Models;

namespace UP02.Elements
{
    /// <summary>
    /// Контрол для отображения и редактирования информации о оборудовании.
    /// </summary>
    public partial class ItemEquipment : UserControl, IRecordDeletable
    {
        /// <summary>
        /// Событие, возникающее при удалении записи.
        /// </summary>
        public event EventHandler RecordDelete;

        Equipment Equipment;
        List<EquipmentLocationHistory> equipmentLocationHistory = new List<EquipmentLocationHistory>();
        List<EquipmentResponsibleHistory> equipmentResponsibleHistory = new List<EquipmentResponsibleHistory>();

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ItemEquipment"/>.
        /// </summary>
        /// <param name="equipment">Оборудование для отображения.</param>
        public ItemEquipment(Equipment equipment)
        {
            InitializeComponent();
            this.Equipment = equipment;
            this.DataContext = equipment;

            UpdateDateGrid();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Удалить". Удаляет оборудование из базы данных, если не связано с другими записями.
        /// </summary>
        private void DeleteClick(object sender, RoutedEventArgs e) 
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                var equipment = databaseContext.Equipment.Where(x => x.EquipmentID == Equipment.EquipmentID).FirstOrDefault();
                if (equipment == null)
                {
                    RecordDelete?.Invoke(this, e);
                    return;
                }

                if (databaseContext.InventoryChecks.Any(x => x.EquipmentID == Equipment.EquipmentID))
                {
                    MessageBox.Show("Нельзя удалить есть связи");
                    return;
                }

                var equipmentLocationHistory = databaseContext.EquipmentLocationHistory.Where(x => x.EquipmentID == equipment.EquipmentID).ToList();
                var equipmentResponsibleHistory = databaseContext.EquipmentResponsibleHistory.Where(x => x.EquipmentID == equipment.EquipmentID).ToList();
                var equipmentConsumables = databaseContext.EquipmentConsumables.Where(x => x.EquipmentID == equipment.EquipmentID).ToList();

                databaseContext.EquipmentLocationHistory.RemoveRange(equipmentLocationHistory);
                databaseContext.EquipmentResponsibleHistory.RemoveRange(equipmentResponsibleHistory);
                databaseContext.EquipmentConsumables.RemoveRange(equipmentConsumables);
                databaseContext.SaveChanges();

                databaseContext.Equipment.Remove(equipment);
                databaseContext.SaveChanges();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            RecordDelete?.Invoke(this, e);
        }

        /// <summary>
        /// Обработчик клика по кнопке "Обновить". Открывает страницу для редактирования оборудования.
        /// </summary>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new Pages.Elements.EditEquipment(Equipment);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного обновления данных оборудования.
        /// </summary>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.Equipment = sender as Equipment;
            this.DataContext = this.Equipment;
            UpdateDateGrid();
        }

        /// <summary>
        /// Обновляет данные в таблицах истории местоположения и ответственного.
        /// </summary>
        void UpdateDateGrid()
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                equipmentLocationHistory = databaseContext.EquipmentLocationHistory.Where(x => x.EquipmentID == Equipment.EquipmentID).Include(a => a.Audience).ToList();
                equipmentResponsibleHistory = databaseContext.EquipmentResponsibleHistory.Where(x => x.EquipmentID == Equipment.EquipmentID).Include(a => a.OldUser).ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
            if (Equipment.Photo != null)
            {
                Photo.Source = UIHelper.ByteArrayToImage(Equipment.Photo);
            }
            locationHistory.ItemsSource = equipmentLocationHistory;
            responsibleUserHistory.ItemsSource = equipmentResponsibleHistory;
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
