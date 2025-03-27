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
using UP02.Elements;
using UP02.Helpers;
using UP02.Models;
using UP02.Pages.Elements;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Логика взаимодействия для PageTypesConsumables.xaml
    /// </summary>
    public partial class PageTypesConsumables : Page
    {
        List<TypesConsumables> OriginalRecords = new List<TypesConsumables>();
        List<TypesConsumables> CurrentList = new List<TypesConsumables>();

        /// <summary>
        /// Конструктор страницы типов расходных материалов. Инициализирует компоненты и загружает данные из базы данных.
        /// </summary>
        public PageTypesConsumables()
        {
            InitializeComponent();
            try
            {
                using var databaseContext = new DatabaseContext();
                OriginalRecords = databaseContext.TypesConsumables.ToList();
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте соединение и повторите попытку.",
                                "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);

                MainWindow.OpenPage(new PageAuthorization());
                return;
            }

            UIHelper.AddItemsToPanel(ContentPanel, OriginalRecords, x => new ItemTypesConsumables(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке "Добавить новую запись". Открывает страницу для редактирования новой записи типа расходного материала.
        /// </summary>
        private void AddNewRecord_Click(object sender, RoutedEventArgs e)
        {
            var editPage = new EditTypesConsumables();
            editPage.RecordSuccess += CreateNewRecordSuccess;
            MainWindow.mainFrame.Navigate(editPage);
        }

        /// <summary>
        /// Обработчик успешного создания новой записи типа расходного материала. Добавляет запись в оригинальный список и сортирует записи.
        /// </summary>
        private void CreateNewRecordSuccess(object sender, EventArgs e)
        {
            var direction = sender as TypesConsumables;
            if (direction == null)
                return;

            OriginalRecords.Add(direction);
            SortRecord();
        }

        private void UpdateRecordSuccess(object sender, EventArgs e)
        {
            var TypesConsumables = sender as TypesConsumables;
            if (TypesConsumables == null)
                return;

            var TypesConsumablesToUpdate = OriginalRecords.Find(x => x.TypeConsumablesID == TypesConsumables.TypeConsumablesID);
            if (TypesConsumablesToUpdate != null)
            {
                // Заменяем старый объект на новый
                int index = OriginalRecords.IndexOf(TypesConsumablesToUpdate);
                OriginalRecords[index] = TypesConsumables;
            }
            SortRecord();
        }

        /// <summary>
        /// Метод сортировки записей типов расходных материалов. Применяет фильтры по поисковому запросу, обновляя отображаемые записи.
        /// </summary>
        private void SortRecord()
        {
            CurrentList = OriginalRecords;

            string searchQuery = SearchField.Text.Trim();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                CurrentList = CurrentList
                    .Where(x => x.Type.IndexOf(searchQuery, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .ToList();
            }

            ContentPanel.Children.Clear();
            UIHelper.AddItemsToPanel(ContentPanel, CurrentList, x => new ItemTypesConsumables(x), OriginalRecords, UpdateRecordSuccess);
        }

        /// <summary>
        /// Обработчик клика по кнопке "Поиск". Перезапускает сортировку с применением фильтра по поисковому запросу.
        /// </summary>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SortRecord();
        }
    }
}
