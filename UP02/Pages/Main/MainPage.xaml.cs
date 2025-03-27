using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using UP02.Context;
using UP02.DocumentProcessing;
using UP02.Helpers;
using UP02.Models;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        // Определяем таблицы, с которыми будем работать
        private enum Tables
        {
            Directions,
            EquipmentModels,
            Statuses,
            TypesConsumables,
            SoftwareDevelopers,
            Users,
            Audiences,
            ConsumableCharacteristics,
            ConsumableCharacteristicValues,
            Equipment,
            Consumables,
            EquipmentConsumables,
            Software,
            Errors,
            Inventories,
            EquipmentLocationHistory,
            EquipmentResponsibleHistory,
            InventoryChecks,
            NetworkSettings,
            TypesEquipment
        }

        private Tables ActiveTable;

        private readonly Dictionary<Tables, Action> tableHandlers;

        public MainPage()
        {
            InitializeComponent();

            // Инициализация словаря обработчиков таблиц
            tableHandlers = new Dictionary<Tables, Action>
            {
                { Tables.Audiences, LoadAudiences },
                { Tables.Directions, LoadDirections },
                { Tables.EquipmentModels, LoadEquipmentModels },
                { Tables.SoftwareDevelopers, LoadSoftwareDevelopers },
                { Tables.Users, LoadUsers },
                { Tables.Software, LoadSoftware },
                { Tables.NetworkSettings, LoadNetworkSettings },
                { Tables.Consumables, LoadConsumables},
                { Tables.TypesConsumables, LoadTypesConsumables },
                { Tables.Equipment, LoadEquipment },
                { Tables.EquipmentConsumables, LoadEquipmentConsumables},
                { Tables.Inventories, LoadInventories },
                { Tables.Statuses, LoadStatuses },
                {Tables.TypesEquipment, LoadTypesEquipment }
            };
        }

        /// <summary>
        /// Универсальный метод загрузки данных для выбранной таблицы.
        /// </summary>
        /// <param name="table">Выбранная таблица.</param>
        private void LoadTable(Tables table)
        {
            ActiveTable = table;
            MainContentPanel.Children.Clear();

            if (tableHandlers.TryGetValue(table, out var handler))
            {
                handler.Invoke();
            }
            else
            {
                MessageBox.Show("Обработчик для выбранной таблицы не найден.");
            }
        }

        /// <summary>
        /// Загрузка аудитории с БД.
        /// </summary>
        /// 

        private void LoadAudiences()
        {
            ContentFrame.Navigate(new PageAudiences());
        }

        /// <summary>
        /// Загрузка направлений.
        /// </summary>
        private void LoadDirections()
        {
            ContentFrame.Navigate(new PageDirections());
        }

        /// <summary>
        /// Загрузка моделей оборудования.
        /// </summary>
        private void LoadEquipmentModels()
        {
            ContentFrame.Navigate(new PageEquipmentModels());
        }

        /// <summary>
        /// Загрузка разработчиков ПО.
        /// </summary>
        private void LoadSoftwareDevelopers()
        {
            ContentFrame.Navigate (new PageSoftwareDevelopers());
        }

        /// <summary>
        /// Загрузка пользователей.
        /// </summary>
        private void LoadUsers()
        {
            ContentFrame.Navigate(new PageUsers());
        }

        /// <summary>
        /// Загрузка ПО.
        /// </summary>
        private void LoadSoftware()
        {
            ContentFrame.Navigate(new PageSoftware());
        }

        /// <summary>
        /// Загрузка сетевых настроек.
        /// </summary>
        private void LoadNetworkSettings()
        {
            ContentFrame.Navigate(new PageNetworkSettings());
        }

        private void LoadConsumables()
        {
            ContentFrame.Navigate(new PageConsumables());
        }

        private void LoadTypesConsumables()
        {
            ContentFrame.Navigate(new PageTypesConsumables());
        }

        private void LoadEquipment()
        {
            ContentFrame.Navigate (new PageEquipment());
        }


        private void LoadTypesEquipment()
        {
            ContentFrame.Navigate(new PageTypesEquipment());
        }
        private void LoadStatuses() 
        {
            ContentFrame.Navigate(new PageStatuses());
        }

        private void LoadEquipmentConsumables()
        {
            ContentFrame.Navigate(new PageEquipmentConsumables());
        }

        private void LoadInventories()
        {
            ContentFrame.Navigate(new PageInventories());
        }

        private void AudiencesButton_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.Audiences);

        private void DirectionsButton_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.Directions);

        private void EquipmentModelsButton_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.EquipmentModels);

        private void SoftwareDevelopersButton_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.SoftwareDevelopers);
        private void UsersButton_Click(object sender, RoutedEventArgs e)
                        => LoadTable(Tables.Users);


        private void TypesEquipment_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.TypesEquipment);

        private void SoftwareButton_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.Software);

        private void NetworkSettingsButton_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.NetworkSettings);

        private void ConsumablesButton_Click(object sender, RoutedEventArgs e) 
            => LoadTable(Tables.Consumables);

        private void TypesConsumablesButton_Click(object sender, RoutedEventArgs e) 
            => LoadTable(Tables.TypesConsumables);

        private void EquipmentButton_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.Equipment);

        private void EquipmentConsumablesButton_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.EquipmentConsumables);

        private void InventoriesButton_Click(object sender, RoutedEventArgs e)
            => LoadTable(Tables.Inventories);

        private void StatusesButton_Click(object sender, RoutedEventArgs e) =>
            LoadTable(Tables.Statuses);


        /// <summary>
        /// Обработчик для импорта данных из файла Excel.
        /// </summary>
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx",
                Title = "Выберите файл Excel",
                Multiselect = false // Разрешаем выбрать только один файл
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                var excelDataImporter = new ExcelDataImporter(selectedFile);
                (string error, List<UserImport> users) = excelDataImporter.ReadData();

                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using var databaseContext = new DatabaseContext();

                try
                {
                    // Выгружаем список существующих пользователей в память для быстрой проверки
                    var existingUserNames = databaseContext.Users
                        .AsNoTracking()
                        .ToList() // Необходим для клиентской фильтрации
                        .Select(u => u.FullName)
                        .ToHashSet();

                    var notExistingUser = users.FirstOrDefault(x => !existingUserNames.Contains(x.FullName));
                    if (notExistingUser != null)
                    {
                        MessageBox.Show($"Пользователь {notExistingUser.FullName} не существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Для каждого импортированного пользователя
                    foreach (var user in users)
                    {
                        // Выгружаем всех пользователей в память и ищем пользователя по полному имени
                        var allUsers = databaseContext.Users.AsNoTracking().ToList();
                        var databaseUser = allUsers.FirstOrDefault(x => x.FullName == user.FullName);
                        if (databaseUser == null)
                        {
                            MessageBox.Show($"Пользователь {user.FullName} не существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        // Прикрепляем найденного пользователя к контексту
                        databaseContext.Attach(databaseUser);

                        // Обработка направлений для пользователя
                        foreach (var TypesEquipment in user.TypesEquipments)
                        {
                            // Ищем направление по имени
                            var databaseTypesEquipment = databaseContext.TypesEquipment.FirstOrDefault(x => x.Name == TypesEquipment.TypesEquipmentName);
                            if (databaseTypesEquipment == null)
                            {
                                // Если направление не найдено, создаём новое
                                databaseTypesEquipment = new TypesEquipment { Name = TypesEquipment.TypesEquipmentName };
                                databaseContext.TypesEquipment.Add(databaseTypesEquipment);
                                databaseContext.SaveChanges();
                            }
                            else
                            {
                                databaseContext.Attach(databaseTypesEquipment);
                            }

                            // Обработка оборудования для каждого направления
                            foreach (var equipment in TypesEquipment.Equipments)
                            {
                                for (int i = 0; i < equipment.Quantity; i++)
                                {
                                    var equipmentForDatabase = new Equipment
                                    {
                                        Name = equipment.Name,
                                        TypeEquipmentID = databaseTypesEquipment.TypeEquipmentID,
                                        TypeEquipment = databaseTypesEquipment,
                                        InventoryNumber = equipment.InventoryNumber,
                                        ResponsibleUserID = databaseUser.UserID,
                                        ResponsibleUser = databaseUser,
                                    };
                                    databaseContext.Equipment.Add(equipmentForDatabase);
                                }
                            }
                        }
                        databaseContext.SaveChanges();
                    }

                    MessageBox.Show($"Данные успешно импортированы.", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    UIHelper.ErrorConnection(databaseContext, ex.Message);
                    return;
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ClearFrame();
            MainWindow.OpenPage(new PageAuthorization());
        }
    }
}