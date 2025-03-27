using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using UP02.Context;
using UP02.Database;
using UP02.Elements;
using UP02.Helpers;
using UP02.Interfaces;
using UP02.Models;

namespace UP02.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для EditInventories.xaml
    /// </summary>
    public partial class EditInventories : Page, IRecordSuccess
    {
        Users? InventoryCreater = null;

        int? InventoryID = null;
        public event EventHandler RecordDelete;
        public event EventHandler RecordSuccess;
        List<InventoryChecks> OriginalInventoryChecks = new List<InventoryChecks>();

        public EditInventories(Inventories inventory = null)
        {
            InitializeComponent();

            List<Users> _Users = new List<Users>();
            List<Equipment> _Equipment = new List<Equipment>();
            List<InventoryChecks> _InventoryChecks = new List<InventoryChecks>();

            using var databaseContext = new DatabaseContext();
            try
            {
                _Users = databaseContext.Users.ToList();

                if (inventory != null)
                {
                    _Equipment = databaseContext.Equipment.Where(x => x.ResponsibleUserID == inventory.UserID || x.TempResponsibleUserID == inventory.UserID).ToList();
                    _InventoryChecks = databaseContext.InventoryChecks.Where(x => x.InventoryID == inventory.InventoryID).ToList();
                }
                else
                {
                    _Equipment = databaseContext.Equipment.Where(x => x.ResponsibleUserID == Settings.CurrentUser.UserID || x.TempResponsibleUserID ==Settings.CurrentUser.UserID).ToList();
                }
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            UsersComboBox.ItemsSource = _Users;
            UsersComboBox.DisplayMemberPath = "FullName";
            UsersComboBox.SelectedValuePath = "UserID";
            UsersComboBox.SelectedValue = Settings.CurrentUser.UserID;

            if (Settings.CurrentUser.Role != "Администратор")
            {
                UsersComboBox.IsEnabled = false;
                NameTextBox.IsEnabled = false;
                StartDate.IsEnabled = false;
                EndDate.IsEnabled = false;
            }

            EquipmentComboBox.ItemsSource = _Equipment;
            EquipmentComboBox.DisplayMemberPath = "Name";
            EquipmentComboBox.SelectedValuePath = "EquipmentID";

            if (inventory != null)
            {
                InventoryCreater = inventory.User;
                InventoryID = inventory.InventoryID;
                NameTextBox.Text = inventory.Name;
                StartDate.Text = inventory.StartDate.ToString("dd.MM.yyyy");
                EndDate.Text = inventory.EndDate.ToString("dd.MM.yyyy");

                InventoryCheckParent.Children.Clear();
                OriginalInventoryChecks = new List<InventoryChecks>(_InventoryChecks.Where(x => x.UserID == Settings.CurrentUser.UserID));

                foreach (var inventoryCheck in OriginalInventoryChecks)
                {
                    var equipment = _Equipment.FirstOrDefault(x => x.EquipmentID == inventoryCheck.EquipmentID);
                    if (equipment == null)
                        continue;

                    var item = new ItemInventoryChecks(equipment, inventoryCheck);
                    item.RecordDelete += Item_RecordDelete;

                    InventoryCheckParent.Children.Add(item);
                }
            }
            else
            {
                InventoryCreater = Settings.CurrentUser;
                CreaterInventories.Text = Settings.CurrentUser.FullName;

                StartDate.Text = DateTime.Now.ToString("dd.MM.yyyy");
                EndDate.Text = DateTime.Now.ToString("dd.MM.yyyy");
            }

            CreaterInventories.Text = InventoryCreater.FullName;
        }


        private void UpdatesFromControls(Inventories inventoryToUpdate, DatabaseContext databaseContext)
        {
            inventoryToUpdate.Name = NameTextBox.Text;
            inventoryToUpdate.User = InventoryCreater;
            inventoryToUpdate.UserID = InventoryCreater.UserID;
            inventoryToUpdate.StartDate = DateTime.Parse(StartDate.Text, CultureInfo.GetCultureInfo("ru-RU"));
            inventoryToUpdate.EndDate = DateTime.Parse(EndDate.Text, CultureInfo.GetCultureInfo("ru-RU"));

            // Проверяем, отслеживается ли уже InventoryCreater в контексте
            var localInventoryCreater = databaseContext.Set<Users>().Local
                                           .FirstOrDefault(u => u.UserID == InventoryCreater.UserID);
            if (localInventoryCreater == null)
            {
                databaseContext.Attach(InventoryCreater);
            }
            else
            {
                // Используем уже отслеживаемый экземпляр
                inventoryToUpdate.User = localInventoryCreater;
            }
        }

        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
                return;

            var checks = InventoryCheckParent.Children.OfType<ItemInventoryChecks>()
                           .Select(x => x.InventoryChecks)
                           .ToList();

            var newChecks = checks.Where(x => x.CheckID == 0).ToList();
            var editChecks = InventoryCheckParent.Children.OfType<ItemInventoryChecks>()
                              .Where(x => x.IsModified)
                              .Select(x => x.InventoryChecks)
                              .Where(x => x.CheckID != 0)
                              .ToList();
            var deleteChecks = OriginalInventoryChecks
                               .Where(x => !checks.Any(f => f.CheckID == x.CheckID))
                               .ToList();

            using var databaseContext = new DatabaseContext();
            try
            {
                var inventoryFromDb = InventoryID.HasValue
                                      ? databaseContext.Inventories.FirstOrDefault(x => x.InventoryID == InventoryID)
                                      : null;
                if (inventoryFromDb == null && InventoryID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                inventoryFromDb ??= new Inventories();
                UpdatesFromControls(inventoryFromDb, databaseContext);

                if (!InventoryID.HasValue)
                {
                    databaseContext.Inventories.Add(inventoryFromDb);
                }

                databaseContext.SaveChanges();

                var user = UsersComboBox.SelectedItem as Users;
                if (user != null)
                {
                    if (user.UserID != InventoryCreater.UserID)
                        databaseContext.Attach(user);
                }

                // Если добавляются или редактируются проверки, можно использовать уже прикрепленного пользователя
                if (newChecks.Count > 0)
                {
                    newChecks.ForEach(x => x.InventoryID = inventoryFromDb.InventoryID);
                    if (user != null)
                    {
                        newChecks.ForEach(x =>
                        {
                            x.UserID = user.UserID;
                        });
                    }
                    databaseContext.InventoryChecks.AddRange(newChecks);
                }

                if (editChecks.Count > 0)
                {
                    foreach (var editCheck in editChecks)
                    {
                        var check = databaseContext.InventoryChecks
                                      .FirstOrDefault(x => x.CheckID == editCheck.CheckID);
                        if (check == null)
                            continue;

                        check.CheckDate = editCheck.CheckDate;
                        check.Comment = editCheck.Comment;
                    }
                }

                if (deleteChecks.Count > 0)
                {
                    databaseContext.InventoryChecks.RemoveRange(deleteChecks);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(inventoryFromDb, EventArgs.Empty);
                MessageBox.Show("Данные успешно сохраненны!", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }


        private bool ValidateAllFields()
        {
            bool incorrect = false;

            // Проверка названия (обязательно, макс. 255 символов)
            incorrect |= UIHelper.ValidateField(NameTextBox.Text, 255, "Название", isRequired: true);

            if (!DateTime.TryParseExact(
                StartDate.Text,
                "dd.MM.yyyy",
                CultureInfo.GetCultureInfo("ru-RU"),
                DateTimeStyles.None,
                out DateTime parsedDate1))
            {
                MessageBox.Show("Указано не верное время начала");
                return true;
            }

            if (!DateTime.TryParseExact(
                EndDate.Text,
                "dd.MM.yyyy",
                CultureInfo.GetCultureInfo("ru-RU"),
                DateTimeStyles.None,
                out DateTime parsedDate2))
            {
                MessageBox.Show("Указано не верное время окончания");
                return true;
            }

            if (parsedDate1 > parsedDate2)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания");
                return true;
            }

            return incorrect;
        }
        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }

        private void EquipmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EquipmentComboBox.SelectedItem == null)
                return;

            var equipment = EquipmentComboBox.SelectedItem as Equipment;

            var item = new ItemInventoryChecks(equipment);
            item.RecordDelete += Item_RecordDelete;

            InventoryCheckParent.Children.Add(item);

            EquipmentComboBox.SelectedItem = null;
        }

        private void Item_RecordDelete(object sender, EventArgs e)
        {
            if (sender is UIElement element && InventoryCheckParent.Children.Contains(element))
            {
                InventoryCheckParent.Children.Remove(element);
            }
        }

        private void UsersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersComboBox.SelectedItem != null && UsersComboBox.SelectedItem is Users user)
            {
                using var databaseContext = new DatabaseContext();
                try
                {
                    InventoryCheckParent.Children.Clear();
                    var _Equipment = databaseContext.Equipment.Where(x => x.ResponsibleUserID == user.UserID || x.TempResponsibleUserID == user.UserID).ToList();
                    EquipmentComboBox.ItemsSource = _Equipment;

                    if (InventoryID.HasValue)
                    {
                        var _InventoryChecks = databaseContext.InventoryChecks.Where(x => x.InventoryID == InventoryID.Value && x.UserID == user.UserID);

                        OriginalInventoryChecks = new List<InventoryChecks>(_InventoryChecks);

                        foreach (var inventoryCheck in _InventoryChecks)
                        {
                            var equipment = _Equipment.FirstOrDefault(x => x.EquipmentID == inventoryCheck.EquipmentID);
                            if (equipment == null)
                                continue;

                            var item = new ItemInventoryChecks(equipment, inventoryCheck);
                            item.RecordDelete += Item_RecordDelete;

                            InventoryCheckParent.Children.Add(item);
                        }
                    }
                }
                catch(Exception ex)
                {
                    UIHelper.ErrorConnection(databaseContext, ex.Message);
                    return;
                }
            }
        }
    }
}
