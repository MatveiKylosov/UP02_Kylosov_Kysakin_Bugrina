using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using UP02.Context;
using UP02.Elements;
using UP02.Helpers;
using UP02.Models;
using UP02.Pages.Elements;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Логика взаимодействия для PageEquipment.xaml
    /// </summary>
    public partial class PageEquipment : Page
    {
        List<Equipment> OriginalRecords = new List<Equipment>();
        List<Equipment> CurrentList = new List<Equipment>();

        /// <summary>
        /// Конструктор страницы, инициализирует компоненты и загружает данные о оборудовании из базы данных.
        /// </summary>
        public PageEquipment()
        {
            InitializeComponent();
            try
            {
                using var databaseContext = new DatabaseContext();
                OriginalRecords = databaseContext.Equipment
                    .Include(a => a.ResponsibleUser)
                    .Include(a => a.TempResponsibleUser)
                    .Include(a => a.Direction)
                    .Include(a => a.Status)
                    .Include(a => a.Model)
                    .Include(a => a.Audience)
                    .Include(a => a.TypeEquipment)
                    .ToList();
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте соединение и повторите попытку.",
                                "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);

                MainWindow.OpenPage(new PageAuthorization());
                return;
            }
            List<Users> users = OriginalRecords
                .SelectMany(a => new[] { a.ResponsibleUser, a.TempResponsibleUser })
                .Where(u => u != null)
                .Distinct()
                .ToList();

            List<EquipmentModels> equipmentModels = OriginalRecords
                .Select(a => a.Model)
                .Where(m => m != null)
                .Distinct()
                .ToList();

            List<Statuses> equipmentStatuses = OriginalRecords
                .Select(a => a.Status)
                .Where(s => s != null)
                .Distinct()
                .ToList();

            List<Directions> equipmentDirections = OriginalRecords
                .Select(a => a.Direction)
                .Where(d => d != null)
                .Distinct()
                .ToList();

            List<Audiences> equipmentAudiences = OriginalRecords
                .Select(a => a.Audience)
                .Where(a => a != null)
                .Distinct()
                .ToList();

            List<TypesEquipment> equipmentTypesEquipment = OriginalRecords
                .Select(a => a.TypeEquipment)
                .Where(a => a != null)
                .Distinct()
                .ToList();


            users.Insert(0, new Users { UserID = -1, LastName = "", FirstName = "Отсутствует", MiddleName = "" });
            equipmentModels.Insert(0, new EquipmentModels { ModelID = -1, Name = "Отсутствует" });
            equipmentStatuses.Insert(0, new Statuses { StatusID = -1, Name = "Отсутствует" });
            equipmentDirections.Insert(0, new Directions { DirectionID = -1, Name = "Отсутствует" });
            equipmentAudiences.Insert(0, new Audiences { AudienceID = -1, ShortName = "Отсутствует" });
            equipmentTypesEquipment.Insert(0, new TypesEquipment { TypeEquipmentID = -1, Name = "Отсутствует" });

            ResponsibleUserCB.ItemsSource = users;
            TempResponsibleUserCB.ItemsSource = users;
            EquipmentModelsCB.ItemsSource = equipmentModels;
            EquipmentStatusesCB.ItemsSource = equipmentStatuses;
            EquipmentAudiencesCB.ItemsSource = equipmentAudiences;
            EquipmentDirectionsCB.ItemsSource = equipmentDirections;
            TypesEquipmentCB.ItemsSource = equipmentTypesEquipment;

            TempResponsibleUserCB.DisplayMemberPath = ResponsibleUserCB.DisplayMemberPath = "FullName";
            TempResponsibleUserCB.SelectedValuePath = ResponsibleUserCB.SelectedValuePath = "UserID";

            EquipmentModelsCB.DisplayMemberPath = "Name";
            EquipmentModelsCB.SelectedValuePath = "ModelID";

            EquipmentStatusesCB.DisplayMemberPath = "Name";
            EquipmentStatusesCB.SelectedValuePath = "StatusID";

            EquipmentAudiencesCB.DisplayMemberPath = "ShortName";
            EquipmentAudiencesCB.SelectedValuePath = "AudienceID";

            EquipmentDirectionsCB.DisplayMemberPath = "Name";
            EquipmentDirectionsCB.SelectedValuePath = "DirectionID";

            TypesEquipmentCB.DisplayMemberPath = "Name";
            TypesEquipmentCB.SelectedValuePath = "TypeEquipmentID";

            TempResponsibleUserCB.SelectedValue = -1;
            ResponsibleUserCB.SelectedValue = -1;
            EquipmentModelsCB.SelectedValue = -1;
            EquipmentStatusesCB.SelectedValue = -1;
            EquipmentDirectionsCB.SelectedValue = -1;
            EquipmentAudiencesCB.SelectedValue = -1;
            TypesEquipmentCB.SelectedValue = -1;

            CurrentList = OriginalRecords;

            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemEquipment(x), OriginalRecords);
        }


        /// <summary>
        /// Обработчик клика по кнопке для добавления нового оборудования.
        /// </summary>

        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditEquipment();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания нового оборудования, добавляет запись в список и сортирует данные.
        /// </summary>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var equipment = sender as Equipment;
            if (equipment == null)
                return;

            OriginalRecords.Add(equipment);
            SortRecord();
        }

        /// <summary>
        /// Метод для сортировки и фильтрации оборудования на основе выбранных значений и поискового запроса.
        /// </summary>
        private void SortRecord()
        {
            CurrentList = OriginalRecords;

            int? selectedResponsible = ResponsibleUserCB.SelectedValue as int?;
            if (selectedResponsible.HasValue && selectedResponsible.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.ResponsibleUserID == selectedResponsible.Value).ToList();
            }

            int? selectedTempResponsible = TempResponsibleUserCB.SelectedValue as int?;
            if (selectedTempResponsible.HasValue && selectedTempResponsible.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.TempResponsibleUserID == selectedTempResponsible.Value).ToList();
            }

            int? selectedModel = EquipmentModelsCB.SelectedValue as int?;
            if(selectedModel.HasValue && selectedModel.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.ModelID == selectedModel.Value).ToList();
            }

            int? selectedStatus = EquipmentStatusesCB .SelectedValue as int?;
            if(selectedStatus.HasValue && selectedStatus.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.StatusID == selectedStatus.Value).ToList();
            }

            int? selectedDirection = EquipmentDirectionsCB.SelectedValue as int?;
            if(selectedDirection.HasValue && selectedDirection.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.DirectionID == selectedDirection.Value).ToList();
            }

            int? selectedAudience = EquipmentAudiencesCB.SelectedValue as int?;
            if(selectedAudience.HasValue && selectedAudience.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.AudienceID == selectedAudience.Value).ToList();
            }

            int? selectedTypeEquipment = TypesEquipmentCB.SelectedValue as int?;
            if (selectedTypeEquipment.HasValue && selectedTypeEquipment.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.TypeEquipmentID == selectedTypeEquipment.Value).ToList();
            }

            string searchQuery = SearchField.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                CurrentList = CurrentList
                    .Where(x => 
                        x.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                        (x.ResponsibleUser != null && x.ResponsibleUser.FullName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                        (x.TempResponsibleUser != null && x.TempResponsibleUser.FullName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                        (x.Direction != null && x.Direction.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                        (x.Status != null && x.Status.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                        (x.Model != null && x.Model.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(x.Comment) && x.Comment.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                        (x.Audience != null && x.Audience.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                        x.InventoryNumber.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                        (x.Cost.HasValue && x.Cost.Value.ToString().IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0)||
                        (x.TypeEquipment != null && x.TypeEquipment.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0)

                    ).ToList();
            }

            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemEquipment(x), OriginalRecords);
        }

        /// <summary>
        /// Обработчик изменения значения в выпадающем списке для сортировки.
        /// </summary>
        private void SortCB_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик клика по кнопке поиска, выполняет сортировку и фильтрацию данных.
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }
    }
}
