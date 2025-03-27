using System.Windows;
using System.Windows.Controls;
using UP02.Context;
using UP02.Database;
using UP02.Helpers;
using UP02.Interfaces;
using UP02.Models;

namespace UP02.Elements
{
    /// <summary>
    /// Контрол для отображения и управления инвентаризацией.
    /// </summary>
    public partial class ItemInventories : UserControl, IRecordDeletable, IRecordUpdatable
    {
        /// <summary>
        /// Событие, возникающее при удалении записи.
        /// </summary>
        public event EventHandler RecordDelete;        public event EventHandler RecordUpdate;
        Inventories Inventory;

        /// <summary>
        /// Создает новый экземпляр <see cref="ItemInventories"/> и инициализирует его.
        /// </summary>
        /// <param name="inventory">Инвентаризация, которая будет отображаться.</param>
        public ItemInventories(Inventories inventory)
        { 
            InitializeComponent(); 
            Inventory = inventory;
            this.DataContext = Inventory;
            DeleteButton.Visibility = Settings.CurrentUser.Role != "Администратор" ? Visibility.Hidden : Visibility.Visible;
        }

        /// <summary>
        /// Обработчик клика по кнопке обновления инвентаризации.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new Pages.Elements.EditInventories(Inventory);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного редактирования инвентаризации.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.Inventory = sender as Inventories;
            this.DataContext = this.Inventory;
            RecordUpdate?.Invoke(Inventory, EventArgs.Empty);
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

        /// <summary>
        /// Обработчик клика по кнопке удаления инвентаризации.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
                using var databaseContext = new DatabaseContext();
            try
            {
                if (!databaseContext.InventoryChecks.Any(x => x.InventoryID == Inventory.InventoryID))
                {
                    var inventory = databaseContext.Inventories.FirstOrDefault(x => x.InventoryID == Inventory.InventoryID);

                    databaseContext.Inventories.Remove(inventory);
                    databaseContext.SaveChanges();
                    RecordDelete?.Invoke(this, e);
                }
                else
                {
                    MessageBox.Show("Нельзя удалить есть связи");
                }
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
        }
    }
}
