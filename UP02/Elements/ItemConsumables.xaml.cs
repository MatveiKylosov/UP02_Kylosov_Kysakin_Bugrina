using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using UP02.Pages.Elements;
using UP02.ViewModels;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace UP02.Elements
{
    /// <summary>
    /// Представляет элемент для отображения и редактирования данных о расходных материалах.
    /// </summary>
    public partial class ItemConsumables : UserControl, IRecordDeletable
    {
        /// <summary>
        /// Событие, вызываемое при удалении записи.
        /// </summary>
        public event EventHandler RecordDelete;
        /// <summary>
        /// Объект расходного материала.
        /// </summary>
        public Consumables Consumable;
        /// <summary>
        /// Коллекция характеристик с их значениями.
        /// </summary>
        ObservableCollection<CharacteristicViewModel> CharacteristicsWithValues = new ObservableCollection<CharacteristicViewModel> ();

        /// <summary>
        /// Флаг, указывающий, если элемент должен быть только для просмотра (без возможности редактировать).
        /// </summary>
        bool ViewElement = false;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ItemConsumables"/> с данными о расходном материале.
        /// </summary>
        /// <param name="Consumable">Объект расходного материала.</param>
        /// <param name="ViewElement">Если true, элементы управления для обновления скрыты.</param>
        public ItemConsumables(Consumables Consumable, bool ViewElement = false)
        { 
            InitializeComponent(); 
            this.Consumable = Consumable;
            this.DataContext = Consumable;
            CharacteristicsWithValues = new ObservableCollection<CharacteristicViewModel>(update());
            CharacteristisDG.ItemsSource = CharacteristicsWithValues;

            if (ViewElement) {
                UpdateButton.Height = 0;
                UpdateButton.Width = 0;
            }

            if (Consumable.Photo != null)
            {
                Photo.Source = UIHelper.ByteArrayToImage(Consumable.Photo);
            }
            this.ViewElement = ViewElement;

        }

        /// <summary>
        /// Обрабатывает клик по кнопке удаления.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (!ViewElement)
            {
                using var databaseContext = new DatabaseContext();
                try
                {
                    var consumable = databaseContext.Consumables.FirstOrDefault(c => c.ConsumableID == Consumable.ConsumableID);
                    if (consumable == null)
                    {
                        RecordDelete?.Invoke(this, e);
                        return;
                    }

                    if (databaseContext.EquipmentConsumables.Any(x => x.ConsumableID == consumable.ConsumableID))
                    {
                        MessageBox.Show("Нельзя удалить есть связи.");
                        return;
                    }

                    var consumableCharacteristicValues = databaseContext.ConsumableCharacteristicValues.Where(x => x.ConsumablesID == Consumable.ConsumableID).ToList();
                    if (consumableCharacteristicValues.Any())
                    {
                        databaseContext.RemoveRange(consumableCharacteristicValues);
                    }

                    databaseContext.Consumables.Remove(consumable);

                    databaseContext.SaveChanges();
                }
                catch(Exception ex)
                {
                    UIHelper.ErrorConnection(databaseContext, ex.Message);
                    return;
                }
            }
            
            RecordDelete?.Invoke(this, e);
        }

        /// <summary>
        /// Обрабатывает клик по кнопке обновления.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new EditConsumables(Consumable);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обновляет данные расходного материала и его характеристики после успешного редактирования.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.Consumable = sender as Consumables;
            this.DataContext = this.Consumable;

            CharacteristicsWithValues = new ObservableCollection<CharacteristicViewModel>(update());

            CharacteristisDG.ItemsSource = CharacteristicsWithValues;

            if (Consumable.Photo != null)
            {
                Photo.Source = UIHelper.ByteArrayToImage(Consumable.Photo);
            }
        }

        /// <summary>
        /// Обновляет коллекцию характеристик с их значениями.
        /// </summary>
        /// <returns>Список обновленных характеристик с их значениями.</returns>
        List<CharacteristicViewModel> update()
        {
            List<CharacteristicViewModel> characteristicsWithValues = new List<CharacteristicViewModel>();

            using var databaseContext = new DatabaseContext();
            try
            {
                if (Consumable == null || Consumable.TypeConsumablesID == null)
                    return new List<CharacteristicViewModel>();

                var typeConsumables = databaseContext.TypesConsumables
                    .FirstOrDefault(x => x.TypeConsumablesID == Consumable.TypeConsumablesID);

                if (typeConsumables == null)
                    return new List<CharacteristicViewModel>();

                characteristicsWithValues = databaseContext.ConsumableCharacteristics
                    .Where(c => c.TypeConsumablesID == typeConsumables.TypeConsumablesID)
                    .GroupJoin(
                        databaseContext.ConsumableCharacteristicValues
                            .Where(v => v.ConsumablesID == Consumable.ConsumableID && v.CharacteristicID.HasValue),
                        c => c.CharacteristicID,
                        v => v.CharacteristicID,
                        (c, values) => new CharacteristicViewModel(
                            c.CharacteristicID,
                            values.FirstOrDefault().CharacteristicsValueID,
                            Consumable.ConsumableID,
                            c.CharacteristicName,
                            values.FirstOrDefault().Value)
                    )
                    .ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
            }

            return characteristicsWithValues;
        }

        /// <summary>
        /// Обрабатывает событие удаления записи из элемента.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void _RecordDelete(object sender, EventArgs e)
        {
            RecordDelete?.Invoke(this, e);
        }
        private void ExportConsumablesClick(object sender, RoutedEventArgs e)
        {
            string currentDate = DateTime.Now.ToString("dd.MM.yyyy");
            string fileName = $"Акт_приёма-передачи_{Consumable.Name}_{currentDate.Replace(".", "_")}.docx";
            using (var context = new DatabaseContext())
            {
                var responsibleUser = context.Users.FirstOrDefault(u => u.UserID == Consumable.TempResponsibleUserID);
                if (responsibleUser == null)
                {
                    MessageBox.Show("Информация об ответственном отсутствует.");
                    return;
                }
                using (DocX document = DocX.Create(fileName))
                {
                    document.InsertParagraph("АКТ\nприёма-передачи расходных материалов\n\n").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    var locationAndDate = document.InsertParagraph($"г. Пермь {currentDate}\n\n").Font("Times New Roman").FontSize(12).Alignment = Alignment.left;
                    string fullName = $"{responsibleUser.LastName} {responsibleUser.FirstName[0]}.{responsibleUser.MiddleName[0]}.";
                    var mainText = document.InsertParagraph(
                        $"КГАПОУ Пермский Авиационный техникум им. А.Д. Швецова в целях " +
                        $"обеспечения необходимым оборудованием для исполнения должностных " +
                        $"обязанностей передаёт сотруднику {fullName}, а сотрудник " +
                        $"принимает от учебного учреждения следующие расходные материалы:\n\n").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    var consumableInfo = document.InsertParagraph(
                        $"{Consumable.TypeConsumables?.Type} \"{Consumable.Name}\", " +
                        $"в количестве {Consumable.Quantity ?? 1} шт.\n\n").Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                    var signature = document.InsertParagraph(
                        $"\n\n{responsibleUser.LastName} {responsibleUser.FirstName[0]}.{responsibleUser.MiddleName[0]}. " +
                        $"____________________\n\n" +
                        $"М.П.").Font("Times New Roman").FontSize(12).Alignment = Alignment.left;
                    try
                    {
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string filePath = System.IO.Path.Combine(desktopPath, fileName);
                        document.SaveAs(filePath);
                        MessageBox.Show($"Генерация прошла успешно. Путь к файлу: {filePath}");
                        Process.Start(new ProcessStartInfo(filePath)
                        {
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении документа: {ex.Message}");
                    }
                }
            }
        }
    }
}
