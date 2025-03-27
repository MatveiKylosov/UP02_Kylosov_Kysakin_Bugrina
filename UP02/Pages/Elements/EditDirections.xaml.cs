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
    public partial class EditDirections : Page, IRecordSuccess
    {
        int? DirectionID = null;
        public event EventHandler RecordDelete;
        public event EventHandler RecordSuccess;

        public EditDirections(Directions direction = null)
        {
            InitializeComponent();
            if (direction != null)
            {
                Name.Text = direction.Name;
                DirectionID = direction.DirectionID;
            }
        }

        private bool ValidateAllFields()
        {
            bool incorrect = false;
            incorrect |= UIHelper.ValidateField(Name.Text, 255, "Название", isRequired: true);

            return incorrect;
        }

        private void UpdatesFromControls(Directions directionToUpdate)
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
                Directions directionFromdatabaseContext = null;

                if (DirectionID.HasValue)
                {
                    directionFromdatabaseContext = databaseContext.Directions.FirstOrDefault(d => d.DirectionID == DirectionID.Value);
                }

                if (directionFromdatabaseContext == null && DirectionID.HasValue)
                {
                    MessageBox.Show(
                        "Запись в базе данных не найдена.",
                        "Запись не найдена",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    RecordDelete?.Invoke(null, EventArgs.Empty);
                    return;
                }

                if (directionFromdatabaseContext == null)
                {
                    directionFromdatabaseContext = new Directions();
                }

                UpdatesFromControls(directionFromdatabaseContext);

                if (!DirectionID.HasValue)
                {
                    databaseContext.Directions.Add(directionFromdatabaseContext);
                }

                databaseContext.SaveChanges();

                RecordSuccess?.Invoke(directionFromdatabaseContext, EventArgs.Empty);

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
