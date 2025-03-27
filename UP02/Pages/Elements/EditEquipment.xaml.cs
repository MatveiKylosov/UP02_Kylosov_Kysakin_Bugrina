using System.IO;
using System.Windows;
using System.Windows.Controls;
using UP02.Context;
using UP02.Helpers;
using UP02.Interfaces;
using UP02.Models;

namespace UP02.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для EditEquipment.xaml
    /// </summary>
    public partial class EditEquipment : Page, IRecordSuccess
    {
        int? EquipmentID = null;
        public event EventHandler RecordDelete;
        public event EventHandler RecordSuccess;
        int? CurrentAudiences = null;
        int? CurrentResponsibleUser = null;
        byte[]? imageBytes;

        public EditEquipment(Equipment equipment = null)
        {
            InitializeComponent();

            List<Users> _Users = new List<Users>();
            List<Statuses> _Statuses = new List<Statuses>();
            List<Directions> _Directions = new List<Directions>();
            List<EquipmentModels> _Models = new List<EquipmentModels>();
            List<Audiences> _Audiences = new List<Audiences>();
            List<TypesEquipment> _TypesEquipment = new List<TypesEquipment>();

            using var databaseContext = new DatabaseContext();
            try
            {
                _Users = databaseContext.Users.ToList();
                _Statuses = databaseContext.Statuses.ToList();
                _Directions = databaseContext.Directions.ToList();
                _Models = databaseContext.EquipmentModels.ToList();
                _Audiences = databaseContext.Audiences.ToList();
                _TypesEquipment = databaseContext.TypesEquipment.ToList();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }

            _Users.Insert(0, new Users { UserID = -1, LastName = "", FirstName = "Отсутствует", MiddleName = "" });
            _Statuses.Insert(0, new Statuses { StatusID = -1, Name = "Отсутствует" });
            _Directions.Insert(0, new Directions { DirectionID = -1, Name = "Отсутствует" });
            _Models.Insert(0, new EquipmentModels { ModelID = -1, Name = "Отсутствует" });
            _Audiences.Insert(0, new Audiences { AudienceID = -1, Name = "Отсутствует" });
            _TypesEquipment.Insert(0, new TypesEquipment { TypeEquipmentID = -1, Name = "Отсутствует" });


            ResponsibleUserCB.ItemsSource = _Users;
            TempResponsibleUserCB.ItemsSource = _Users;

            StatusCB.ItemsSource = _Statuses;
            DirectionCB.ItemsSource = _Directions;
            ModelCB.ItemsSource = _Models;
            AudienceCB.ItemsSource = _Audiences;
            TypesEquipmentCB.ItemsSource = _TypesEquipment;

            TempResponsibleUserCB.DisplayMemberPath = ResponsibleUserCB.DisplayMemberPath = "FullName";
            TempResponsibleUserCB.SelectedValuePath = ResponsibleUserCB.SelectedValuePath = "UserID";

            StatusCB.DisplayMemberPath = "Name";
            DirectionCB.DisplayMemberPath = "Name";
            ModelCB.DisplayMemberPath = "Name";
            AudienceCB.DisplayMemberPath = "Name";
            TypesEquipmentCB.DisplayMemberPath = "Name";

            StatusCB.SelectedValuePath = "StatusID";
            DirectionCB.SelectedValuePath = "DirectionID";
            ModelCB.SelectedValuePath = "ModelID";
            AudienceCB.SelectedValuePath = "AudienceID";
            TypesEquipmentCB.SelectedValuePath = "TypeEquipmentID";

            ResponsibleUserCB.SelectedValue = -1;
            TempResponsibleUserCB.SelectedValue = -1;
            DirectionCB.SelectedValue = -1;
            StatusCB.SelectedValue = -1;
            ModelCB.SelectedValue = -1;
            AudienceCB.SelectedValue = -1;
            TypesEquipmentCB.SelectedValue = -1;

            if (equipment != null)
            {
                EquipmentID = equipment?.EquipmentID;

                TextBoxName.Text = equipment.Name;
                TextBoxComment.Text = equipment.Comment;
                TextBoxInventoryNumber.Text = equipment.InventoryNumber;
                TextBoxCost.Text = equipment.Cost.ToString();
                imageBytes = equipment.Photo;

                ResponsibleUserCB.SelectedValue = equipment.ResponsibleUserID ?? -1;
                TempResponsibleUserCB.SelectedValue = equipment.TempResponsibleUserID ?? -1;
                DirectionCB.SelectedValue = equipment.DirectionID ?? -1;
                StatusCB.SelectedValue = equipment.StatusID ?? -1;
                ModelCB.SelectedValue = equipment.ModelID ?? -1;
                TypesEquipmentCB.SelectedValue = equipment.TypeEquipmentID ?? -1;
                AudienceCB.SelectedValue = equipment.AudienceID ?? -1;

                CurrentAudiences = equipment.AudienceID;
                CurrentResponsibleUser = equipment.ResponsibleUserID;
            }
        }

        private bool ValidateAllFields()
        {
            bool incorrect = false;

            // Проверка обязательного поля "Название" (максимум 255 символов)
            incorrect |= UIHelper.ValidateField(TextBoxName.Text, 255, "Название", isRequired: true);

            // Проверка обязательного поля "Инвентарный номер" (максимум 50 символов, только цифры)
            incorrect |= UIHelper.ValidateField(TextBoxInventoryNumber.Text, 50, "Инвентарный номер", regexPattern: "^[0-9]+$", isRequired: true);

            // Если поле "Стоимость" заполнено, проверяем, что оно является числом не меньше 0.
            if (!decimal.TryParse(TextBoxCost.Text, out decimal cost))
            {
                MessageBox.Show("Поле \"Стоимость\" должно быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                incorrect = true;
            }

            var responsibleUserCBSelectedValue = ResponsibleUserCB.SelectedValue as int?;
            var tempResponsibleUserCBSelectedValue = TempResponsibleUserCB.SelectedValue as int?;
            if ((!responsibleUserCBSelectedValue.HasValue || responsibleUserCBSelectedValue == -1) && (!tempResponsibleUserCBSelectedValue.HasValue || tempResponsibleUserCBSelectedValue.Value == -1))
            {
                MessageBox.Show("Не выбран ни один ответственный пользователь. Выберите либо ответственного пользователя, либо временно ответственного.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                incorrect = true;
            }

            // Если необходимо, можно также добавить проверки для других ComboBox-ов (AudienceCB, DirectionCB, StatusCB, ModelCB).

            return incorrect;
        }

        private void UpdatesFromControls(Equipment equipmentToUpdate, DatabaseContext databaseContext)
        {

            equipmentToUpdate.Name = TextBoxName.Text;
            equipmentToUpdate.Comment = TextBoxComment.Text;
            equipmentToUpdate.InventoryNumber = TextBoxInventoryNumber.Text;
            equipmentToUpdate.Cost = decimal.Parse(TextBoxCost.Text);
            equipmentToUpdate.Photo = imageBytes;

            // Ответственный пользователь
            UIHelper.SetEntity<Users, int?>(
                ResponsibleUserCB,
                id => equipmentToUpdate.ResponsibleUserID = id,
                entity => equipmentToUpdate.ResponsibleUser = entity,
                u => u.UserID,
                -1,
                databaseContext.Users);

            // Временный ответственный пользователь
            UIHelper.SetEntity<Users, int?>(
                TempResponsibleUserCB,
                id => equipmentToUpdate.TempResponsibleUserID = id,
                entity => equipmentToUpdate.TempResponsibleUser = entity,
                u => u.UserID,
                -1,
                databaseContext.Users);

            // Направление
            UIHelper.SetEntity<Directions, int?>(
                DirectionCB,
                id => equipmentToUpdate.DirectionID = id,
                entity => equipmentToUpdate.Direction = entity,
                d => d.DirectionID,
                -1,
                databaseContext.Directions);

            // Статус
            UIHelper.SetEntity<Statuses, int?>(
                StatusCB,
                id => equipmentToUpdate.StatusID = id,
                entity => equipmentToUpdate.Status = entity,
                s => s.StatusID,
                -1,
                databaseContext.Statuses);

            // Модель оборудования
            UIHelper.SetEntity<EquipmentModels, int?>(
                ModelCB,
                id => equipmentToUpdate.ModelID = id,
                entity => equipmentToUpdate.Model = entity,
                m => m.ModelID,
                -1,
                databaseContext.EquipmentModels);

            // Аудитория
            UIHelper.SetEntity<Audiences, int?>(
                AudienceCB,
                id => equipmentToUpdate.AudienceID = id,
                entity => equipmentToUpdate.Audience = entity,
                a => a.AudienceID,
                -1,
                databaseContext.Audiences);

            UIHelper.SetEntity<TypesEquipment, int?>(
                TypesEquipmentCB,
                id => equipmentToUpdate.TypeEquipmentID = id,
                entity => equipmentToUpdate.TypeEquipment = entity,
                a => a.TypeEquipmentID,
                -1,
                databaseContext.TypesEquipment);
        }

        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
                return;

            using var databaseContext = new DatabaseContext();
            try
            {
                var equipmentFromdatabaseContext = EquipmentID.HasValue
                    ? databaseContext.Equipment.FirstOrDefault(a => a.EquipmentID == EquipmentID.Value)
                    : null;

                if (equipmentFromdatabaseContext == null && EquipmentID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                equipmentFromdatabaseContext ??= new Equipment();

                UpdatesFromControls(equipmentFromdatabaseContext, databaseContext);

                if (EquipmentID != null)
                {
                    if (CurrentAudiences.HasValue && CurrentAudiences != equipmentFromdatabaseContext.AudienceID)
                    {
                        databaseContext.EquipmentLocationHistory.Add(
                            new EquipmentLocationHistory()
                            {
                                EquipmentID = equipmentFromdatabaseContext.EquipmentID,
                                ChangeDate = DateTime.Now,
                                Comment = equipmentFromdatabaseContext.Comment,
                                AudienceID = CurrentAudiences,
                            }
                            );
                    }

                    if (CurrentResponsibleUser.HasValue && CurrentResponsibleUser != equipmentFromdatabaseContext.ResponsibleUserID)
                    {
                        databaseContext.EquipmentResponsibleHistory.Add(
                            new EquipmentResponsibleHistory()
                            {
                                EquipmentID = equipmentFromdatabaseContext.EquipmentID,
                                OldUserID = CurrentResponsibleUser,
                                ChangeDate = DateTime.Now,
                                Comment = equipmentFromdatabaseContext.Comment,
                            }
                        );
                    }
                }

                if (!EquipmentID.HasValue)
                {
                    databaseContext.Equipment.Add(equipmentFromdatabaseContext);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(equipmentFromdatabaseContext, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }

            MainWindow.mainFrame.GoBack();
        }

        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }

        private void SelectAndSaveImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Получаем массив байтов из выбранного файла
                imageBytes = File.ReadAllBytes(openFileDialog.FileName);
            }
        }
    }
}
