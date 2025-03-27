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
    /// Логика взаимодействия для EditSoftware.xaml
    /// </summary>
    /// <summary>
    /// Страница редактирования программного обеспечения
    /// </summary>
    public partial class EditSoftware : Page, IRecordSuccess
    {
        /// <summary>
        /// Идентификатор программного обеспечения (null для нового ПО)
        /// </summary>
        int? SoftwareID = null;

        /// <summary>
        /// Событие удаления записи о программном обеспечении
        /// </summary>
        public event EventHandler RecordDelete;

        /// <summary>
        /// Событие успешного сохранения записи
        /// </summary>
        public event EventHandler RecordSuccess;

        /// <summary>
        /// Конструктор страницы редактирования ПО
        /// </summary>
        /// <param name="software">Редактируемое ПО (null для создания нового)</param>
        public EditSoftware(Software software = null)
        {
            InitializeComponent();

            List<SoftwareDevelopers> softwareDevelopers = new List<SoftwareDevelopers>();
            List<Equipment> equipment = new List<Equipment>();

            using var databaseContext = new DatabaseContext();
            try
            {
                softwareDevelopers = databaseContext.SoftwareDevelopers.ToList();
                equipment = databaseContext.Equipment.ToList();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }

            softwareDevelopers.Insert(0, new SoftwareDevelopers { DeveloperID = -1, Name = "Отсутствует" });
            equipment.Insert(0, new Equipment { EquipmentID = -1, Name = "Отсутствует" });

            SoftwareDeveloperCB.ItemsSource = softwareDevelopers;
            EquipmentCB.ItemsSource = equipment;

            SoftwareDeveloperCB.DisplayMemberPath = EquipmentCB.DisplayMemberPath = "Name";
            SoftwareDeveloperCB.SelectedValuePath = "DeveloperID";
            EquipmentCB.SelectedValuePath = "EquipmentID";

            if (software != null)
            {
                this.SoftwareID = software.SoftwareID;
                Name.Text = software.Name;
                this.Version.Text = software.Version;
            }
            SoftwareDeveloperCB.SelectedValue = software?.DeveloperID ?? -1;
            EquipmentCB.SelectedValue = software?.EquipmentID ?? -1;
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
        /// Обновляет данные ПО из элементов управления
        /// </summary>
        /// <param name="softwareToUpdate">Объект ПО для обновления</param>
        /// <param name="databaseContext">Контекст базы данных</param>
        private void UpdatesFromControls(Software softwareToUpdate, DatabaseContext databaseContext)
        {
            softwareToUpdate.Name = Name.Text;
            softwareToUpdate.Version = Version.Text;

            var softwareDeveloper = SoftwareDeveloperCB.SelectedItem as SoftwareDevelopers;
            if (softwareDeveloper != null && softwareDeveloper.DeveloperID != -1)
            {
                softwareToUpdate.DeveloperID = softwareDeveloper.DeveloperID;
                databaseContext.SoftwareDevelopers.Attach(softwareDeveloper);
                softwareToUpdate.Developer = softwareDeveloper;
            }
            else
            {
                softwareToUpdate.DeveloperID = null;
                softwareToUpdate.Developer = null;
            }

            var equipment = EquipmentCB.SelectedItem as Equipment;
            if (equipment != null && equipment.EquipmentID != -1)
            {
                softwareToUpdate.EquipmentID = equipment.EquipmentID;
                databaseContext.Equipment.Attach(equipment);
                softwareToUpdate.Equipment = equipment;
            }
            else
            {
                softwareToUpdate.EquipmentID = null;
                softwareToUpdate.Equipment = null;
            }
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
                var softwareFromDb = SoftwareID.HasValue
                    ? databaseContext.Software.FirstOrDefault(a => a.SoftwareID == SoftwareID.Value)
                    : null;

                if (softwareFromDb == null && SoftwareID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                softwareFromDb ??= new Software();

                UpdatesFromControls(softwareFromDb, databaseContext);

                if (!SoftwareID.HasValue)
                {
                    databaseContext.Software.Add(softwareFromDb);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(softwareFromDb, EventArgs.Empty);

                MainWindow.mainFrame.GoBack();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
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