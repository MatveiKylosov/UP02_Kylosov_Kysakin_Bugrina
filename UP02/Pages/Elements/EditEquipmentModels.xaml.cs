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
    /// Логика взаимодействия для EditEquipmentModels.xaml
    /// </summary>
    public partial class EditEquipmentModels : Page, IRecordSuccess
    {
        int? EquipmentModelID = null;
        public event EventHandler RecordDelete;
        public event EventHandler RecordSuccess;

        public EditEquipmentModels(EquipmentModels equipmentModels = null)
        {
            InitializeComponent();
            if (equipmentModels != null)
            {
                Name.Text = equipmentModels.Name;
                EquipmentModelID = equipmentModels.ModelID;
            }
        }

        private bool ValidateAllFields()
        {
            bool incorrect = false;
            incorrect |= UIHelper.ValidateField(Name.Text, 255, "Название", isRequired: true);

            return incorrect;
        }

        private void UpdatesFromControls(EquipmentModels equipmentModelToUpdate)
        {
            equipmentModelToUpdate.Name = Name.Text;
        }

        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
                return;

            using var databaseContext = new DatabaseContext();
            try
            {

                var equipmentModelFromDb = EquipmentModelID.HasValue
                    ? databaseContext.EquipmentModels.FirstOrDefault(em => em.ModelID == EquipmentModelID.Value)
                    : null;

                if (equipmentModelFromDb == null && EquipmentModelID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                equipmentModelFromDb ??= new EquipmentModels();

                UpdatesFromControls(equipmentModelFromDb);

                if (!EquipmentModelID.HasValue)
                {
                    databaseContext.EquipmentModels.Add(equipmentModelFromDb);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(equipmentModelFromDb, EventArgs.Empty);

                MainWindow.mainFrame.GoBack();

            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
        }

        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }
    }
}
