using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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
using UP02.Elements;
using UP02.Helpers;
using UP02.Interfaces;
using UP02.Models;
using UP02.ViewModels;

namespace UP02.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для EditConsumables.xaml
    /// </summary>
    public partial class EditConsumables : Page, IRecordSuccess
    {
        int? ConsumableID = null;
        int TypeConsumablesID = -1;
        bool TypeConsumableChange = false;
        bool HaveDataOfCharacteristic = false;
        byte[] imageBytes;

        public event EventHandler RecordDelete;
        public event EventHandler RecordSuccess;
        ObservableCollection<CharacteristicViewModel> characteristicViewModels = new ObservableCollection<CharacteristicViewModel>();

        public EditConsumables(Consumables consumable = null)
        {
            InitializeComponent();

            List<Users> users = new List<Users>();
            List<TypesConsumables> typesConsumables = new List<TypesConsumables>();

            using var databaseContext = new DatabaseContext();
            try
            {
                users = databaseContext.Users.ToList();
                typesConsumables = databaseContext.TypesConsumables.ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            users.Insert(0, new Users { UserID = -1, LastName = "", FirstName = "Отсутствует", MiddleName = "" });
            typesConsumables.Insert(0, new TypesConsumables { TypeConsumablesID = -1, Type = "Отсутствует" });

            TempResponsibleUserCB.ItemsSource = users;
            TypesConsumablesCB.ItemsSource = typesConsumables;

            TempResponsibleUserCB.DisplayMemberPath = "FullName";
            TempResponsibleUserCB.SelectedValuePath = "UserID";

            TypesConsumablesCB.DisplayMemberPath = "Type";
            TypesConsumablesCB.SelectedValuePath = "TypeConsumablesID";

            if(consumable != null)
            {
                ConsumableID = consumable.ConsumableID;
                NameTB.Text = consumable.Name;
                DescriptionTB.Text = consumable.Description;
                ReceiptDateTB.Text = ReceiptDateTB.Text = consumable.ReceiptDate?.ToString("dd.MM.yyyy") ?? "";
                QuantityTB.Text = consumable.Quantity?.ToString() ?? "";

                if (consumable.TypeConsumablesID.HasValue) 
                {
                    try
                    {
                        HaveDataOfCharacteristic = databaseContext.ConsumableCharacteristicValues.Any(x => x.ConsumablesID == ConsumableID);
                    }
                    catch(Exception ex)
                    {
                        UIHelper.ErrorConnection(databaseContext, ex.Message);
                        return;
                    }

                    UpdateDataAndTypesConsumablesCB(consumable.TypeConsumablesID.Value);
                }
            }

            TempResponsibleUserCB.SelectedValue = consumable?.TempResponsibleUserID ?? -1;
            TypesConsumablesCB.SelectedValue = TypeConsumablesID = consumable?.TypeConsumablesID ?? -1;
        }

        private bool ValidateAllFields()
        {
            bool incorrect = false;

            // Проверка названия (обязательно, макс. 255 символов)
            incorrect |= UIHelper.ValidateField(NameTB.Text, 255, "Название", isRequired: true);

            // Проверка описания (не обязательно)
            incorrect |= UIHelper.ValidateField(DescriptionTB.Text, 255, "Описание", isRequired: false);

            // Проверка даты получения (не обязательно, ожидаемый формат "dd.MM.yyyy", длина 10 символов)
            incorrect |= UIHelper.ValidateField(ReceiptDateTB.Text, 10, "Дата получения", isRequired: false);

            // Проверка количества (должно быть целым числом не меньше 0)
            if (!String.IsNullOrEmpty(QuantityTB.Text) && (!int.TryParse(QuantityTB.Text, out int quantity) || quantity < 0))
            {
                incorrect = true;
                MessageBox.Show("Количество должно быть неотрицательным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            incorrect |= DateTime.TryParseExact(
                this.ReceiptDateTB.Text,
                "dd.MM.yyyy HH:mm",
                CultureInfo.GetCultureInfo("ru-RU"),
                DateTimeStyles.None,
                out DateTime parsedDate);

            return incorrect;
        }

        private void UpdatesFromControls(Consumables consumableToUpdate, DatabaseContext databaseContext)
        {
            consumableToUpdate.Name = NameTB.Text;
            consumableToUpdate.Description = DescriptionTB.Text;

            var tempResponsibleUser = TempResponsibleUserCB.SelectedItem as Users;
            if (tempResponsibleUser != null && tempResponsibleUser.UserID != -1)
            {
                consumableToUpdate.TempResponsibleUserID = tempResponsibleUser.UserID;
                databaseContext.Users.Attach(tempResponsibleUser);
                consumableToUpdate.TempResponsibleUser = tempResponsibleUser;
            }
            else
            {
                consumableToUpdate.TempResponsibleUserID = null;
                consumableToUpdate.TempResponsibleUser = null;
            }

            var typesConsumables = TypesConsumablesCB.SelectedItem as TypesConsumables;
            if (typesConsumables != null && typesConsumables.TypeConsumablesID != -1)
            {
                consumableToUpdate.TypeConsumablesID = typesConsumables.TypeConsumablesID;
                databaseContext.TypesConsumables.Attach(typesConsumables);
                consumableToUpdate.TypeConsumables = typesConsumables;
            }
            else
            {
                consumableToUpdate.TypeConsumablesID = null;
                consumableToUpdate.TypeConsumables = null;
            }

            consumableToUpdate.ReceiptDate = DateTime.Parse(ReceiptDateTB.Text, CultureInfo.GetCultureInfo("ru-RU"));
            consumableToUpdate.Quantity = int.Parse(QuantityTB.Text);
            consumableToUpdate.Photo = imageBytes;
        }

        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
                return;

            using var databaseContext = new DatabaseContext();
            try
            {
                var consumableFromdatabaseContext = ConsumableID.HasValue
                    ? databaseContext.Consumables.FirstOrDefault(c => c.ConsumableID == ConsumableID.Value)
                    : null;

                if (consumableFromdatabaseContext == null && ConsumableID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                if (consumableFromdatabaseContext != null)
                {
                    if (TypeConsumableChange)
                    {
                        var consumableCharacteristicValues = databaseContext.ConsumableCharacteristicValues.Where(x => x.ConsumablesID == consumableFromdatabaseContext.ConsumableID).ToList();
                        databaseContext.ConsumableCharacteristicValues.RemoveRange(consumableCharacteristicValues);
                    }

                    var changedValueCharacteristics = characteristicViewModels.Where(x => x.ValueChanged && x.CharacteristicsValueID != null).ToList();
                    if (changedValueCharacteristics.Any()) 
                    {
                        foreach (var characteristic in changedValueCharacteristics) {
                            var newValue = characteristic.Value;

                            var databaseCharacteristic = databaseContext.ConsumableCharacteristicValues.FirstOrDefault(x => x.CharacteristicsValueID == characteristic.CharacteristicsValueID);
                            if (databaseCharacteristic == null)
                                continue;

                            databaseCharacteristic.Value = newValue;
                        }
                    }

                    var newCharacteristics = characteristicViewModels.Where(x => x.CharacteristicsValueID == null && x.Value != null).ToList();
                    if (newCharacteristics.Any())
                    {
                        foreach (var characteristic in newCharacteristics)
                        {
                            var newCharacteristic = new ConsumableCharacteristicValues
                            {
                                CharacteristicID = characteristic.CharacteristicID,
                                ConsumablesID = characteristic.ConsumablesID,
                                Value = characteristic.Value
                            };

                            databaseContext.ConsumableCharacteristicValues.Add(newCharacteristic);
                        }
                    }
                }


                consumableFromdatabaseContext ??= new Consumables();

                UpdatesFromControls(consumableFromdatabaseContext, databaseContext);

                if (!ConsumableID.HasValue)
                {
                    databaseContext.Consumables.Add(consumableFromdatabaseContext);
                }

                databaseContext.SaveChanges();

                if (!ConsumableID.HasValue)
                {
                    var newCharacteristics = characteristicViewModels.Where(x => x.CharacteristicsValueID == null && x.Value != null).ToList();
                    if (newCharacteristics.Any())
                    {
                        foreach (var characteristic in newCharacteristics)
                        {
                            var newCharacteristic = new ConsumableCharacteristicValues
                            {
                                CharacteristicID = characteristic.CharacteristicID,
                                ConsumablesID = consumableFromdatabaseContext.ConsumableID,
                                Value = characteristic.Value
                            };

                            databaseContext.ConsumableCharacteristicValues.Add(newCharacteristic);
                        }
                    }
                    databaseContext.SaveChanges();
                }

                RecordSuccess?.Invoke(consumableFromdatabaseContext, EventArgs.Empty);

                MainWindow.mainFrame.GoBack();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }

        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }

        private void TypesConsumablesCB_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is TypesConsumables typesConsumables)
            {
                TypeConsumableChange = !(typesConsumables.TypeConsumablesID == TypeConsumablesID && TypeConsumablesID != -1);
                UpdateDataAndTypesConsumablesCB(typesConsumables.TypeConsumablesID);
            }
        }

        private void UpdateDataAndTypesConsumablesCB(int TypeConsumablesID)
        {
            if (TypeConsumablesID == -1)
            {
                characteristicViewModels.Clear();
            }
            else
            {
                using var databaseContext = new DatabaseContext();
                try
                {
                    var typeConsumables = databaseContext.TypesConsumables
                        .FirstOrDefault(x => x.TypeConsumablesID == TypeConsumablesID);

                    if (typeConsumables == null)
                        return;

                    var characteristicsWithValues = databaseContext.ConsumableCharacteristics
                        .Where(c => c.TypeConsumablesID == typeConsumables.TypeConsumablesID)
                        .GroupJoin(
                            databaseContext.ConsumableCharacteristicValues
                                .Where(v => v.ConsumablesID == ConsumableID && v.CharacteristicID.HasValue),
                            c => c.CharacteristicID,
                            v => v.CharacteristicID,
                            (c, values) => new CharacteristicViewModel(
                                c.CharacteristicID,
                                values.FirstOrDefault().CharacteristicsValueID,
                                ConsumableID,
                                c.CharacteristicName,
                                values.FirstOrDefault().Value)
                        )
                        .ToList();
                    characteristicViewModels = new ObservableCollection<CharacteristicViewModel>(characteristicsWithValues);
                }
                catch(Exception ex)
                {
                    UIHelper.ErrorConnection(databaseContext, ex.Message);
                    return;
                }
            }
            CharacteristisDG.ItemsSource = characteristicViewModels;
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
