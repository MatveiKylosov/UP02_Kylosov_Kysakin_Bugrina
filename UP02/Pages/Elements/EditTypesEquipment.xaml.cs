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
using UP02.Models;
using Xceed.Document.NET;

namespace UP02.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для EditTypesEquipment.xaml
    /// </summary>
    public partial class EditTypesEquipment : Page
    {
        int? TypeEquipmentID = null;
        public event EventHandler RecordDelete;
        public event EventHandler RecordSuccess;
        public EditTypesEquipment(TypesEquipment typesEquipment = null)
        {
            InitializeComponent();
            if (typesEquipment != null)
            {
                Name.Text = typesEquipment.Name;
                TypeEquipmentID = typesEquipment.TypeEquipmentID;
            }
        }
        private bool ValidateAllFields()
        {
            bool incorrect = false;
            incorrect |= UIHelper.ValidateField(Name.Text, 255, "Название", isRequired: true);

            return incorrect;
        }

        private void UpdatesFromControls(TypesEquipment directionToUpdate)
        {
            directionToUpdate.Name = Name.Text;
        }

        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
                return;

            using var databaseContext = new DatabaseContext();
            try
            {
                TypesEquipment typesEquipmentFromdatabaseContext = null;

                if (TypeEquipmentID.HasValue)
                {
                    typesEquipmentFromdatabaseContext = databaseContext.TypesEquipment.FirstOrDefault(d => d.TypeEquipmentID == TypeEquipmentID.Value);
                }

                if (typesEquipmentFromdatabaseContext == null && TypeEquipmentID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                if (typesEquipmentFromdatabaseContext == null)
                {
                    typesEquipmentFromdatabaseContext = new TypesEquipment();
                }

                UpdatesFromControls(typesEquipmentFromdatabaseContext);

                if (!TypeEquipmentID.HasValue)
                {
                    databaseContext.TypesEquipment.Add(typesEquipmentFromdatabaseContext);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(typesEquipmentFromdatabaseContext, EventArgs.Empty);

                MainWindow.mainFrame.GoBack();
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }

        private void UndoСhangesClick(object sender, RoutedEventArgs e)
        {
            MainWindow.mainFrame.GoBack();
        }
    }
}
