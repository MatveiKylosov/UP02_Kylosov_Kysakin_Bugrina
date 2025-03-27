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

namespace UP02.Elements
{
    /// <summary>
    /// Логика взаимодействия для ItemDirections.xaml
    /// </summary>
    public partial class ItemDirections : UserControl
    {
        public ItemDirections(Directions Direction)
        {
            InitializeComponent();

            this.Direction = Direction;
            this.DataContext = Direction;
        }

        public event EventHandler RecordDelete;
        Directions Direction;

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                if (databaseContext.Equipment.Any(e => e.DirectionID == Direction.DirectionID))
                {
                    MessageBox.Show("Нельзя удалить, есть связи");
                    return;
                }

                var direction = databaseContext.Directions.FirstOrDefault(x => x.DirectionID == Direction.DirectionID);
                if (direction != null)
                {
                    databaseContext.Directions.Remove(direction);
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
            var editPage = new Pages.Elements.EditDirections(Direction);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        private void EditSuccess(object sender, EventArgs e)
        {
            this.Direction = sender as Directions;
            this.DataContext = this.Direction;
        }
        private void _RecordDelete(object sender, EventArgs e)
        {
            RecordDelete?.Invoke(this, e);
        }
    }
}
