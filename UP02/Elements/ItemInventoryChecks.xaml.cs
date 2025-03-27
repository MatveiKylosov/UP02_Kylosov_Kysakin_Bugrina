using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using UP02.Interfaces;
using UP02.Models;

namespace UP02.Elements
{
    /// <summary>
    /// Логика взаимодействия для ItemInventoryChecks.xaml
    /// </summary>
    /// <summary>
    /// Контрол для отображения и управления проверками инвентаризации оборудования.
    /// </summary>
    public partial class ItemInventoryChecks : UserControl, IRecordDeletable
    {
        private Equipment Equipment;
        private InventoryChecks? _originalInventoryChecks;

        /// <summary>
        /// Получает или устанавливает объект <see cref="InventoryChecks"/>.
        /// </summary>
        public InventoryChecks? InventoryChecks
        {
            private set
            {
                _originalInventoryChecks = value;
            }
            get
            {
                // Создаем новый объект InventoryChecks на основе данных с UI
                InventoryChecks inventoryChecks = new InventoryChecks
                {
                    EquipmentID = Equipment.EquipmentID,
                    Comment = CommentTextBox.Text
                };

                // Если существуют исходные данные, добавляем их в новый объект
                if (_originalInventoryChecks != null)
                {
                    inventoryChecks.CheckID = _originalInventoryChecks.CheckID;
                    inventoryChecks.InventoryID = _originalInventoryChecks.InventoryID;
                }

                // Преобразуем строку даты и времени в DateTime
                if (!DateTime.TryParseExact(CheckDateTextBox.Text, "HH:mm dd.MM.yyyy",
                    CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out DateTime parsedDate))
                    return null;

                inventoryChecks.CheckDate = parsedDate;
                return inventoryChecks;
            }
        }

        /// <summary>
        /// Определяет, были ли изменения в данных инвентаризации.
        /// </summary>
        public bool IsModified
        {
            get
            {
                if (InventoryChecks == null) return false;
                if (_originalInventoryChecks == null) return false;

                // Сравниваем только дату и время, игнорируя секунды и миллисекунды
                var originalCheckDate = _originalInventoryChecks.CheckDate;
                var newCheckDate = InventoryChecks.CheckDate;

                // Обрезаем секунды и миллисекунды
                originalCheckDate = originalCheckDate.AddSeconds(-originalCheckDate.Second);
                newCheckDate = newCheckDate.AddSeconds(-newCheckDate.Second);

                return (newCheckDate != originalCheckDate) || (InventoryChecks.Comment != _originalInventoryChecks.Comment);
            }
        }

        /// <summary>
        /// Событие, возникающее при удалении записи.
        /// </summary>
        public event EventHandler RecordDelete;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ItemInventoryChecks"/>.
        /// </summary>
        /// <param name="Equipment">Оборудование, к которому относится проверка.</param>
        /// <param name="InventoryChecks">Проверка инвентаризации (если имеется), для редактирования.</param>
        public ItemInventoryChecks(Equipment Equipment, InventoryChecks InventoryChecks = null)
        {
            InitializeComponent();
            this.Equipment = Equipment;
            EquipmentNameLabel.Content = Equipment.Name;

            // Устанавливаем начальные значения для UI
            if (InventoryChecks != null)
            {
                CheckDateTextBox.Text = InventoryChecks.CheckDate.ToString("HH:mm dd.MM.yyyy");
                CommentTextBox.Text = InventoryChecks.Comment;
            }
            else
            {
                CheckDateTextBox.Text = DateTime.Now.ToString("HH:mm dd.MM.yyyy");
                CommentTextBox.Text = "";
            }

            this.InventoryChecks = InventoryChecks;

            // Сохраняем исходные данные для сравнения
            _originalInventoryChecks = InventoryChecks != null ? new InventoryChecks
            {
                EquipmentID = InventoryChecks.EquipmentID,
                CheckID = InventoryChecks.CheckID,
                InventoryID = InventoryChecks.InventoryID,
                CheckDate = InventoryChecks.CheckDate,
                Comment = InventoryChecks.Comment,
                UserID = InventoryChecks.UserID,
            } : null;
        }

        /// <summary>
        /// Обработчик клика по кнопке удаления проверки инвентаризации.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            RecordDelete?.Invoke(this, e);
        }

        /// <summary>
        /// Обработчик потери фокуса для текстового поля даты и времени проверки.
        /// Проверяет правильность введенной даты и времени.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void CheckDateTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!DateTime.TryParse(CheckDateTextBox.Text, out _))
            {
                MessageBox.Show("Неправильная дата и время");
                CheckDateTextBox.Text = "";
            }
        }
    }
}
