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
    /// <summary>
    /// Логика взаимодействия для EditSoftwareDevelopers.xaml
    /// </summary>
    public partial class EditSoftwareDevelopers : Page, IRecordSuccess
    {
        /// <summary>
        /// Идентификатор разработчика (null для нового)
        /// </summary>
        int? DeveloperID = null;

        /// <summary>
        /// Событие удаления записи разработчика
        /// </summary>
        public event EventHandler RecordDelete;

        /// <summary>
        /// Событие успешного сохранения записи
        /// </summary>
        public event EventHandler RecordSuccess;

        /// <summary>
        /// Конструктор страницы редактирования разработчика ПО
        /// </summary>
        /// <param name="SoftwareDeveloper">Редактируемый разработчик (null для нового)</param>
        public EditSoftwareDevelopers(SoftwareDevelopers SoftwareDeveloper = null)
        {
            InitializeComponent();
            if (SoftwareDeveloper != null)
            {
                Name.Text = SoftwareDeveloper.Name;
                DeveloperID = SoftwareDeveloper.DeveloperID;
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
        /// Обновляет данные разработчика из элементов управления
        /// </summary>
        /// <param name="softwareDeveloperToUpdate">Объект разработчика для обновления</param>
        private void UpdatesFromControls(SoftwareDevelopers softwareDeveloperToUpdate)
        {
            softwareDeveloperToUpdate.Name = Name.Text;
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
                var SoftwareDeveloperFromDb = DeveloperID.HasValue
                    ? databaseContext.SoftwareDevelopers.FirstOrDefault(sd => sd.DeveloperID == DeveloperID.Value)
                    : null;

                if (SoftwareDeveloperFromDb == null && DeveloperID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                SoftwareDeveloperFromDb ??= new SoftwareDevelopers();

                UpdatesFromControls(SoftwareDeveloperFromDb);

                if (!DeveloperID.HasValue)
                {
                    databaseContext.SoftwareDevelopers.Add(SoftwareDeveloperFromDb);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(SoftwareDeveloperFromDb, EventArgs.Empty);

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
