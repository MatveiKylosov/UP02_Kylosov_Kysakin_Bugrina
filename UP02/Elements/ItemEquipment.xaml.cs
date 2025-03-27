using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using UP02.Context;
using UP02.Helpers;
using UP02.Interfaces;
using UP02.Models;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace UP02.Elements
{
    /// <summary>
    /// Контрол для отображения и редактирования информации о оборудовании.
    /// </summary>
    public partial class ItemEquipment : UserControl, IRecordDeletable, IRecordUpdatable
    {
        /// <summary>
        /// Событие, возникающее при удалении записи.
        /// </summary>
        public event EventHandler RecordDelete;        public event EventHandler RecordUpdate;

        Equipment Equipment;
        List<EquipmentLocationHistory> equipmentLocationHistory = new List<EquipmentLocationHistory>();
        List<EquipmentResponsibleHistory> equipmentResponsibleHistory = new List<EquipmentResponsibleHistory>();

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ItemEquipment"/>.
        /// </summary>
        /// <param name="equipment">Оборудование для отображения.</param>
        public ItemEquipment(Equipment equipment)
        {
            InitializeComponent();
            this.Equipment = equipment;
            this.DataContext = equipment;

            UpdateDateGrid();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Удалить". Удаляет оборудование из базы данных, если не связано с другими записями.
        /// </summary>
        private void DeleteClick(object sender, RoutedEventArgs e) 
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                var equipment = databaseContext.Equipment.Where(x => x.EquipmentID == Equipment.EquipmentID).FirstOrDefault();
                if (equipment == null)
                {
                    RecordDelete?.Invoke(this, e);
                    return;
                }

                if (databaseContext.InventoryChecks.Any(x => x.EquipmentID == Equipment.EquipmentID))
                {
                    MessageBox.Show("Нельзя удалить есть связи");
                    return;
                }

                var equipmentLocationHistory = databaseContext.EquipmentLocationHistory.Where(x => x.EquipmentID == equipment.EquipmentID).ToList();
                var equipmentResponsibleHistory = databaseContext.EquipmentResponsibleHistory.Where(x => x.EquipmentID == equipment.EquipmentID).ToList();
                var equipmentConsumables = databaseContext.EquipmentConsumables.Where(x => x.EquipmentID == equipment.EquipmentID).ToList();

                databaseContext.EquipmentLocationHistory.RemoveRange(equipmentLocationHistory);
                databaseContext.EquipmentResponsibleHistory.RemoveRange(equipmentResponsibleHistory);
                databaseContext.EquipmentConsumables.RemoveRange(equipmentConsumables);
                databaseContext.SaveChanges();

                databaseContext.Equipment.Remove(equipment);
                databaseContext.SaveChanges();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            RecordDelete?.Invoke(this, e);
        }

        /// <summary>
        /// Обработчик клика по кнопке "Обновить". Открывает страницу для редактирования оборудования.
        /// </summary>
        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            var editPage = new Pages.Elements.EditEquipment(Equipment);
            editPage.RecordSuccess += EditSuccess;
            editPage.RecordDelete += _RecordDelete;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного обновления данных оборудования.
        /// </summary>
        private void EditSuccess(object sender, EventArgs e)
        {
            this.Equipment = sender as Equipment;
            this.DataContext = this.Equipment;
            UpdateDateGrid();
            RecordUpdate?.Invoke(Equipment, EventArgs.Empty);
        }

        /// <summary>
        /// Обновляет данные в таблицах истории местоположения и ответственного.
        /// </summary>
        void UpdateDateGrid()
        {
            using var databaseContext = new DatabaseContext();
            try
            {
                equipmentLocationHistory = databaseContext.EquipmentLocationHistory.Where(x => x.EquipmentID == Equipment.EquipmentID).Include(a => a.Audience).ToList();
                equipmentResponsibleHistory = databaseContext.EquipmentResponsibleHistory.Where(x => x.EquipmentID == Equipment.EquipmentID).Include(a => a.OldUser).ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }
            if (Equipment.Photo != null)
            {
                Photo.Source = UIHelper.ByteArrayToImage(Equipment.Photo);
            }
            locationHistory.ItemsSource = equipmentLocationHistory;
            responsibleUserHistory.ItemsSource = equipmentResponsibleHistory;
        }

        /// <summary>
        /// Обработчик события удаления записи.
        /// </summary>
        private void _RecordDelete(object sender, EventArgs e)
        {
            RecordDelete?.Invoke(this, e);
        }
        /// <summary>
        /// Обработчик клика по кнопке "Экспорт Акт приёма-передачи". Генерирует документ в формате Word.
        /// </summary>
        private void ExportAcceptanceClick(object sender, RoutedEventArgs e)
        {
            string currentDate = DateTime.Now.ToString("dd.MM.yyyy");
            using var databaseContext = new DatabaseContext();
            Equipment? equipment;

            try
            {
                equipment = databaseContext.Equipment
                    .Include(e => e.Model)
                    .Include(e => e.ResponsibleUser)
                    .FirstOrDefault(e => e.EquipmentID == Equipment.EquipmentID);
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            if (equipment == null)
            {
                MessageBox.Show("Оборудование не найдено в базе данных.");
                return;
            }

            var responsibleUser = equipment.TempResponsibleUser ?? equipment.ResponsibleUser;
            if (responsibleUser == null)
            {
                MessageBox.Show("Не указан ответственный за оборудование.");
                return;
            }

            string fileName = $"Акт_приёма-передачи_{equipment.Name}_{currentDate.Replace(".", "_")}.docx";
            using (DocX document = DocX.Create(fileName))
            {
                var title = document.InsertParagraph("АКТ\nприема-передачи оборудования\n\n");
                title.Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                var locationDate = document.InsertParagraph($"г. Пермь {currentDate}\n\n");
                locationDate.Font("Times New Roman").FontSize(12).Alignment = Alignment.left;
                string userFullName = $"{responsibleUser.LastName} {responsibleUser.FirstName[0]}.{responsibleUser.MiddleName[0]}.";
                var mainText = document.InsertParagraph(
                    $"КГАПОУ Пермский Авиационный техникум им. А.Д. Швецова в целях " +
                    $"обеспечения необходимым оборудованием для исполнения должностных " +
                    $"обязанностей передаёт сотруднику {userFullName}, а сотрудник " +
                    $"принимает от учебного учреждения следующее оборудование:\n\n");
                mainText.Font("Times New Roman").FontSize(12).Alignment = Alignment.both;
                var equipmentInfo = document.InsertParagraph(
                    $"{equipment.Name}, {equipment.Model?.Name}, " +
                    $"серийный номер {equipment.InventoryNumber}, " +
                    $"стоимостью {equipment.Cost?.ToString("N2")} руб.\n\n\n");
                equipmentInfo.Font("Times New Roman").FontSize(12).Alignment = Alignment.left;
                var signature = document.InsertParagraph(
                    $"{responsibleUser.LastName} {responsibleUser.FirstName[0]}.{responsibleUser.MiddleName[0]}. " +
                    "____________________     ________________");
                signature.Font("Times New Roman").FontSize(12).Alignment = Alignment.left;
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
                    UIHelper.ErrorConnection(databaseContext, ex.Message);
                    MessageBox.Show($"Ошибка при сохранении документа: {ex.Message}");
                    return;
                }
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке "Экспорт Акт приёма-передачи (временно)". Генерирует документ в формате Word.
        /// </summary>
        private void ExportTemporarilyAcceptanceClick(object sender, RoutedEventArgs e)
        {
            string currentDate = DateTime.Now.ToString("dd.MM.yyyy");
            Equipment? equipment;
            using var databaseContext = new DatabaseContext();
            try
            {
                equipment = databaseContext.Equipment
                .Include(e => e.Model)
                .Include(e => e.TempResponsibleUser)
                .FirstOrDefault(e => e.EquipmentID == Equipment.EquipmentID);
            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            if (equipment == null)
            {
                MessageBox.Show("Оборудование не найдено в базе данных.");
                return;
            }
            if (equipment.TempResponsibleUser == null)
            {
                MessageBox.Show("Не указан временно ответственный за оборудование.");
                return;
            }
            string fileName = $"Акт_приёма-передачи_временное_{equipment.Name}_{currentDate.Replace(".", "_")}.docx";
            using (DocX document = DocX.Create(fileName))
            {
                var title = document.InsertParagraph("АКТ\nприема-передачи оборудования на временное пользование\n\n");
                title.Font("Times New Roman").FontSize(12).Alignment = Alignment.center;
                var locationDate = document.InsertParagraph($"г. Пермь {currentDate}\n\n");
                locationDate.Font("Times New Roman").FontSize(12).Alignment = Alignment.left;
                string userFullName = $"{equipment.TempResponsibleUser.LastName} " +
                                    $"{equipment.TempResponsibleUser.FirstName[0]}." +
                                    $"{equipment.TempResponsibleUser.MiddleName[0]}.";
                var mainText = document.InsertParagraph(
                    $"КГАПОУ Пермский Авиационный техникум им. А.Д. Швецова в целях " +
                    $"обеспечения необходимым оборудованием для исполнения должностных " +
                    $"обязанностей передаёт сотруднику {userFullName}, а сотрудник " +
                    $"принимает от учебного учреждения следующее оборудование:\n\n");
                mainText.Font("Times New Roman").FontSize(12).Alignment = Alignment.both;
                var equipmentInfo = document.InsertParagraph(
                    $"{equipment.Name}, {equipment.Model?.Name}, " +
                    $"серийный номер {equipment.InventoryNumber}, " +
                    $"стоимостью {equipment.Cost?.ToString("N2")} руб.\n\n");
                equipmentInfo.Font("Times New Roman").FontSize(12).Alignment = Alignment.left;
                var returnText = document.InsertParagraph(
                    "По окончанию должностных работ «__» ____________ 20___ года, работник " +
                    "обязуется вернуть полученное оборудование.\n\n");
                returnText.Font("Times New Roman").FontSize(12).Alignment = Alignment.both;
                var signature = document.InsertParagraph(
                    $"{equipment.TempResponsibleUser.LastName} " +
                    $"{equipment.TempResponsibleUser.FirstName[0]}." +
                    $"{equipment.TempResponsibleUser.MiddleName[0]}. " +
                    "____________________     ________________");
                signature.Font("Times New Roman").FontSize(12).Alignment = Alignment.left;
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
                    UIHelper.ErrorConnection(databaseContext, ex.Message);
                    MessageBox.Show($"Ошибка при сохранении документа: {ex.Message}");
                    return;
                }
            }
        }
    }
}
