using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace UP02.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для EditTypesConsumables.xaml
    /// </summary>
    public partial class EditTypesConsumables : Page, IRecordSuccess
    {
        /// <summary>
        /// Текущий редактируемый тип расходного материала
        /// </summary>
        private TypesConsumables? _currentTypeConsumables;

        /// <summary>
        /// Событие удаления записи типа расходного материала
        /// </summary>
        public event EventHandler RecordDelete;

        /// <summary>
        /// Событие успешного сохранения изменений
        /// </summary>
        public event EventHandler RecordSuccess;

        /// <summary>
        /// Коллекция характеристик для отображения в DataGrid
        /// </summary>
        private ObservableCollection<ConsumableCharacteristics> DataGridCharacteristics { get; set; } = new ObservableCollection<ConsumableCharacteristics>();

        /// <summary>
        /// Оригинальный список характеристик (для сравнения изменений)
        /// </summary>
        private List<ConsumableCharacteristics> _originalCharacteristics = new List<ConsumableCharacteristics>();

        /// <summary>
        /// Конструктор страницы редактирования типа расходных материалов
        /// </summary>
        /// <param name="typeConsumables">Редактируемый тип расходного материала (null при создании нового)</param>
        public EditTypesConsumables(TypesConsumables? typeConsumables = null)
        {
            InitializeComponent();

            if (typeConsumables != null)
            {
                _currentTypeConsumables = typeConsumables;
                TypeTB.Text = typeConsumables.Type;
            }

            UpdateData();
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки сохранения
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (_currentTypeConsumables == null)
                AddTypeConsumablesFunc();
            else
                EditTypesConsumablesFunc();

            RecordSuccess?.Invoke(_currentTypeConsumables, EventArgs.Empty);
            MainWindow.mainFrame.GoBack();
        }

        /// <summary>
        /// Добавляет новый тип расходного материала в базу данных
        /// </summary>
        private void AddTypeConsumablesFunc()
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                _currentTypeConsumables = new TypesConsumables();
                _currentTypeConsumables.Type = TypeTB.Text;

                databaseContext.TypesConsumables.Add(_currentTypeConsumables);
                databaseContext.SaveChanges();

                var added = DataGridCharacteristics.ToList();
                if (added != null)
                {
                    added.RemoveAll(a => string.IsNullOrEmpty(a.CharacteristicName));
                    added.ForEach(cc => cc.TypeConsumablesID = _currentTypeConsumables.TypeConsumablesID);
                    databaseContext.ConsumableCharacteristics.AddRange(added);
                    databaseContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Редактирует существующий тип расходного материала
        /// </summary>
        private void EditTypesConsumablesFunc()
        {
            var added = DataGridCharacteristics
                .Where(dc => !_originalCharacteristics.Any(oc => oc.CharacteristicID == dc.CharacteristicID))
                .ToList();

            var modified = DataGridCharacteristics
                .Where(dc =>
                {
                    var original = _originalCharacteristics.FirstOrDefault(oc => oc.CharacteristicID == dc.CharacteristicID);
                    return original != null && original.CharacteristicName != dc.CharacteristicName;
                })
                .ToList();

            using var databaseContext = new DatabaseContext();
            try
            {
                if (TypeTB.Text != _currentTypeConsumables.Type)
                {
                    var typeConsumables = databaseContext.TypesConsumables
                        .FirstOrDefault(tc => tc.TypeConsumablesID == _currentTypeConsumables.TypeConsumablesID);
                    if (typeConsumables != null)
                    {
                        typeConsumables.Type = TypeTB.Text;
                    }
                    else
                    {
                        MessageBox.Show(
                            "Запись в базе данных не найдена.",
                            "Запись не найдена",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        RecordDelete?.Invoke(null, EventArgs.Empty);
                        return;
                    }
                }

                if (added.Any())
                {
                    added.RemoveAll(a => string.IsNullOrEmpty(a.CharacteristicName));
                    added.ForEach(cc => cc.TypeConsumablesID = _currentTypeConsumables.TypeConsumablesID);
                    databaseContext.ConsumableCharacteristics.AddRange(added);
                }

                if (modified.Any())
                {
                    modified.RemoveAll(a => string.IsNullOrEmpty(a.CharacteristicName));
                    foreach (var modifiedItem in modified)
                    {
                        var itemToUpdate = databaseContext.ConsumableCharacteristics
                            .FirstOrDefault(c => c.CharacteristicID == modifiedItem.CharacteristicID);
                        if (itemToUpdate != null)
                        {
                            itemToUpdate.CharacteristicName = modifiedItem.CharacteristicName;
                        }
                    }
                }

                databaseContext.SaveChanges();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки отмены
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }

        /// <summary>
        /// Обрабатывает удаление характеристик по нажатию Delete
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void CharacteristisDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && CharacteristisDG.SelectedItem is ConsumableCharacteristics selectedItem)
            {
                bool isNew = !_originalCharacteristics.Any(oc => oc.CharacteristicID == selectedItem.CharacteristicID);
                if (isNew)
                {
                    DataGridCharacteristics.Remove(selectedItem);
                    e.Handled = true;
                    return;
                }

                using var databaseContext = new DatabaseContext();
                try
                {
                    var consumableCharacteristic = databaseContext.ConsumableCharacteristics
                        .FirstOrDefault(x => x.CharacteristicID == selectedItem.CharacteristicID);
                    if (consumableCharacteristic != null)
                    {
                        if (!databaseContext.ConsumableCharacteristicValues.Any(ccv => ccv.CharacteristicID == selectedItem.CharacteristicID))
                        {
                            databaseContext.ConsumableCharacteristics.Remove(consumableCharacteristic);
                            DataGridCharacteristics.Remove(selectedItem);
                        }
                        else
                        {
                            MessageBox.Show("Нельзя удалить характеристику, так как имеются связанные значения.");
                        }
                    }
                    else
                    {
                        DataGridCharacteristics.Remove(selectedItem);
                    }
                    databaseContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    UIHelper.ErrorConnection(ex.Message);
                    return;
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Обновляет данные в интерфейсе
        /// </summary>
        void UpdateData()
        {
            if (_currentTypeConsumables != null)
            {
                using var databaseContext = new DatabaseContext();
                try
                {
                    _originalCharacteristics = databaseContext.ConsumableCharacteristics
                        .Where(cc => cc.TypeConsumablesID == _currentTypeConsumables.TypeConsumablesID)
                        .ToList();
                }
                catch (Exception ex)
                {
                    UIHelper.ErrorConnection(ex.Message);
                    return;
                }

                DataGridCharacteristics.Clear();
                foreach (var item in _originalCharacteristics)
                {
                    DataGridCharacteristics.Add(new ConsumableCharacteristics
                    {
                        CharacteristicID = item.CharacteristicID,
                        TypeConsumablesID = item.TypeConsumablesID,
                        CharacteristicName = item.CharacteristicName,
                    });
                }
            }

            CharacteristisDG.ItemsSource = DataGridCharacteristics;
        }
    }
}