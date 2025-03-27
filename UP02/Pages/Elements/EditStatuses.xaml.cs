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

namespace UP02.Pages.Elements
{
    public partial class EditStatuses : Page, IRecordSuccess
    {
        /// <summary>
        /// Идентификатор статуса (null для нового статуса)
        /// </summary>
        int? StatusID = null;

        /// <summary>
        /// Событие удаления записи статуса
        /// </summary>
        public event EventHandler RecordDelete;

        /// <summary>
        /// Событие успешного сохранения записи
        /// </summary>
        public event EventHandler RecordSuccess;

        /// <summary>
        /// Конструктор страницы редактирования статуса
        /// </summary>
        /// <param name="status">Редактируемый статус (null для нового)</param>
        public EditStatuses(Statuses status = null)
        {
            InitializeComponent();
            if (status != null)
            {
                Name.Text = status.Name;
                StatusID = status.StatusID;
            }
        }

        /// <summary>
        /// Проверяет корректность заполнения полей формы
        /// </summary>
        /// <returns>True если есть ошибки валидации</returns>
        private bool ValidateAllFields()
        {
            bool incorrect = false;
            incorrect |= UIHelper.ValidateField(Name.Text, 255, "Название", isRequired: true);
            return incorrect;
        }

        /// <summary>
        /// Обновляет данные статуса из элементов управления
        /// </summary>
        /// <param name="statusToUpdate">Объект статуса для обновления</param>
        private void UpdatesFromControls(Statuses statusToUpdate)
        {
            statusToUpdate.Name = Name.Text;
        }

        /// <summary>
        /// Обрабатывает сохранение изменений
        /// </summary>
        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
                return;

            using var databaseContext = new DatabaseContext();
            try
            {
                var stastuFromDb = StatusID.HasValue
                    ? databaseContext.Statuses.FirstOrDefault(s => s.StatusID == StatusID.Value)
                    : null;

                if (stastuFromDb == null && StatusID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                stastuFromDb ??= new Statuses();

                UpdatesFromControls(stastuFromDb);

                if (!StatusID.HasValue)
                {
                    databaseContext.Statuses.Add(stastuFromDb);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(stastuFromDb, EventArgs.Empty);

                MainWindow.mainFrame.GoBack();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }

        /// <summary>
        /// Обрабатывает отмену изменений
        /// </summary>
        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }
    }
}
