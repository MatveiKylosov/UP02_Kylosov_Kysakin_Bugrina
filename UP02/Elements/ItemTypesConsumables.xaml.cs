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
    /// Логика взаимодействия для ItemTypesConsumables.xaml
    /// </summary>
    public partial class ItemTypesConsumables : UserControl, IRecordDeletable, IRecordUpdatable
    {
        public event EventHandler RecordDelete;        public event EventHandler RecordUpdate;
        private TypesConsumables TypeConsumables;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ItemTypesConsumables"/>.
        /// </summary>
        /// <param name="TypeConsumables">Тип расходного материала для отображения.</param>
        public ItemTypesConsumables(TypesConsumables TypeConsumables)
        {
            InitializeComponent();
            this.DataContext = TypeConsumables;
            this.TypeConsumables = TypeConsumables;
            UpdateData();
        }

        /// <summary>
        /// Обработчик клика по кнопке удаления типа расходного материала.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                // Проверка наличия связей с другими объектами перед удалением
                if (databaseContext.ConsumableCharacteristics.Any(cc => cc.TypeConsumablesID == TypeConsumables.TypeConsumablesID) ||
                    databaseContext.Consumables.Any(c => c.TypeConsumablesID == TypeConsumables.TypeConsumablesID))
                {
                    MessageBox.Show("Нельзя удалить, есть связи");
                    return;
                }

                // Находим и удаляем тип расходного материала из базы данных
                var typeConsumables = databaseContext.TypesConsumables.FirstOrDefault(x => x.TypeConsumablesID == TypeConsumables.TypeConsumablesID);
                if (typeConsumables != null)
                {
                    databaseContext.TypesConsumables.Remove(typeConsumables);
                    databaseContext.SaveChanges();
                }

                // Сигнализируем об удалении записи
                RecordDelete?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                // Обработка ошибок при подключении
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке обновления типа расходного материала.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new Pages.Elements.EditTypesConsumables(TypeConsumables);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик события успешного редактирования типа расходного материала.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.TypeConsumables = sender as TypesConsumables;
            this.DataContext = this.TypeConsumables;
            RecordUpdate?.Invoke(TypeConsumables, EventArgs.Empty);
            // Обновляем данные после успешного редактирования
            UpdateData();
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
        /// Обновляет данные для отображения в таблице характеристик расходных материалов.
        /// </summary>
        private void UpdateData()
        {
            List<ConsumableCharacteristics> consumableCharacteristics = new List<ConsumableCharacteristics>();

            using var databaseContext = new DatabaseContext();
            try
            {
                // Получаем характеристики расходных материалов для текущего типа
                consumableCharacteristics = databaseContext.ConsumableCharacteristics
                    .Where(cc => cc.TypeConsumablesID == TypeConsumables.TypeConsumablesID)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Обработка ошибок при подключении
                UIHelper.ErrorConnection(ex.Message);
                return;
            }

            // Обновляем источник данных для таблицы характеристик
            var result = consumableCharacteristics.ToList();
            CharacteristisDG.ItemsSource = result;
        }
    }
}
