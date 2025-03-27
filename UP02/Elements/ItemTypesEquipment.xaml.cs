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

namespace UP02.Elements
{
    /// <summary>
    /// Логика взаимодействия для ItemTypesEquipment.xaml
    /// </summary>
    public partial class ItemTypesEquipment : UserControl, IRecordDeletable, IRecordUpdatable
    {
        public ItemTypesEquipment(TypesEquipment TypeEquipment)
        {
            InitializeComponent();
            this.TypeEquipment = TypeEquipment;
            this.DataContext = TypeEquipment;
        }

        public event EventHandler RecordDelete;        public event EventHandler RecordUpdate;
        TypesEquipment TypeEquipment;

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                if (databaseContext.Equipment.Any(e => e.TypeEquipmentID == TypeEquipment.TypeEquipmentID))
                {
                    MessageBox.Show("Нельзя удалить, есть связи");
                    return;
                }

                var typeEquipmentID = databaseContext.TypesEquipment.FirstOrDefault(x => x.TypeEquipmentID == TypeEquipment.TypeEquipmentID);
                if (typeEquipmentID != null)
                {
                    databaseContext.TypesEquipment.Remove(typeEquipmentID);
                    databaseContext.SaveChanges();
                }

                RecordDelete?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
        }
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new Pages.Elements.EditTypesEquipment(TypeEquipment);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        private void EditSuccess(object sender, EventArgs e)
        {
            this.TypeEquipment = sender as TypesEquipment;
            this.DataContext = this.TypeEquipment;
            RecordUpdate?.Invoke(TypeEquipment, EventArgs.Empty);
        }
        private void _RecordDelete(object sender, EventArgs e)
        {
            RecordDelete?.Invoke(this, e);
        }
    }
}
