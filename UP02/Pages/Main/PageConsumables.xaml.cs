using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP02.Context;
using UP02.Elements;
using UP02.Helpers;
using UP02.Models;
using UP02.Pages.Elements;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Логика взаимодействия для PageConsumables.xaml
    /// </summary>
    public partial class PageConsumables : Page
    {
        List<Consumables> OriginalRecords = new List<Consumables>();
        List<Consumables> CurrentList = new List<Consumables>();

        /// <summary>
        /// Конструктор страницы, инициализирует компоненты, загружает данные из базы и настраивает элементы управления.
        /// </summary>
        public PageConsumables()
        {
            InitializeComponent();

            using var databaseContext = new DatabaseContext();
            try
            {
                OriginalRecords = databaseContext.Consumables.Include(a => a.TempResponsibleUser)
                            .Include(a => a.TypeConsumables)
                            .ToList();
            }
            catch(Exception ex)
            {
                UIHelper.ErrorConnection(databaseContext, ex.Message);
                return;
            }

            List<Users> users = OriginalRecords.Where(u => u != null && u.TempResponsibleUser != null).Select(e => e.TempResponsibleUser).Distinct().ToList();
            List<TypesConsumables> types = OriginalRecords.Where(u => u != null && u.TypeConsumables != null).Select(e => e.TypeConsumables).Distinct().ToList();

            users.Insert(0, new Users { UserID = -1, LastName = "", FirstName = "Отсутствует", MiddleName = "" });
            types.Insert(0, new TypesConsumables { TypeConsumablesID = -1, Type = "Отсутствует" });

            TempResponsibleUserCB.ItemsSource = users;
            TempResponsibleUserCB.DisplayMemberPath = "FullName";
            TempResponsibleUserCB.SelectedValuePath = "UserID";

            TypeConsumablesCB.ItemsSource = types;
            TypeConsumablesCB.DisplayMemberPath = "Type";
            TypeConsumablesCB.SelectedValuePath = "TypeConsumablesID";

            TempResponsibleUserCB.SelectedValue = -1;
            TypeConsumablesCB.SelectedValue = -1;

            CurrentList = OriginalRecords;

            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemConsumables(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке для добавления нового расходного материала.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditConsumables();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания нового расходного материала, добавляет запись в список и сортирует данные.
        /// </summary>

        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var consumable = sender as Consumables;
            if (consumable == null)
                return;

            OriginalRecords.Add(consumable);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var consumable = sender as Consumables;
            if (consumable == null)
                return;

            var consumableToUpdate = OriginalRecords.Find(x => x.ConsumableID == consumable.ConsumableID);
            if (consumableToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(consumableToUpdate);
                OriginalRecords[index] = consumable;
            }
            SortRecord();
        }

        /// <summary>
        /// Метод для сортировки и фильтрации записей на основе выбранных значений и поискового запроса.
        /// </summary>
        private void SortRecord()
        {
            CurrentList = OriginalRecords;

            int? selectedTempResponsible = TempResponsibleUserCB.SelectedValue as int?;
            if (selectedTempResponsible.HasValue && selectedTempResponsible.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.TempResponsibleUserID == selectedTempResponsible.Value).ToList();
            }

            int? selectedTypeConsumablesCB = TypeConsumablesCB.SelectedValue as int?;
            if (selectedTypeConsumablesCB.HasValue && selectedTypeConsumablesCB.Value != -1)
            {
                CurrentList = CurrentList.Where(x => x.TypeConsumablesID == selectedTypeConsumablesCB.Value).ToList();
            }

            string searchQuery = SearchField.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                CurrentList = CurrentList
                    .Where(x => x.Name.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                (x.Description != null && x.Description.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (x.TempResponsibleUser != null && x.TempResponsibleUser.FullName.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                (x.TypeConsumables != null && x.TypeConsumables.Type.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0)
                          )
                    .ToList();
            }

            if (AfterReceiptDate.SelectedDate.HasValue)
            {
                DateTime afterReceiptDate = AfterReceiptDate.SelectedDate.Value;
                CurrentList = CurrentList.Where(x => x.ReceiptDate >= afterReceiptDate).ToList();
            }

            if (BeforeReceiptDate.SelectedDate.HasValue)
            {
                DateTime beforeReceiptDate = BeforeReceiptDate.SelectedDate.Value;
                CurrentList = CurrentList.Where(x => x.ReceiptDate <= beforeReceiptDate).ToList();
            }

            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemConsumables(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик изменения значения в комбинированном списке сортировки.
        /// </summary>
        private void SortCB_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик клика по кнопке поиска, выполняет сортировку данных.
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик изменения даты в поле выбора даты получения, выполняет сортировку.
        /// </summary>
        private void ReceiptDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик ввода текста в поле выбора даты получения, выполняет сортировку.
        /// </summary>
        private void ReceiptDate_TextInput(object sender, TextCompositionEventArgs e)
        {
            SortRecord();
        }

        /// <summary>
        /// Обработчик потери фокуса в поле выбора даты получения, выполняет сортировку.
        /// </summary>
        private void ReceiptDate_LostFocus(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }
    }
}
