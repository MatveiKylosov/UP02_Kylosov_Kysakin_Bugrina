using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
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
using UP02.Database;
using UP02.Helpers;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Логика взаимодействия для PageAuthorization.xaml
    /// </summary>
    public partial class PageAuthorization : Page
    {
        // Путь к файлу с сохранёнными данными пользователя (логин и пароль)
        private readonly string credentialsFilePath = "credentials.txt";

        /// <summary>
        /// Конструктор страницы авторизации, инициализирует компоненты и загружает сохранённые данные.
        /// </summary>

        public PageAuthorization()
        {
            InitializeComponent();
            LoadCredentialsFromFile();
        }

        /// <summary>
        /// Загрузка данных (логин и пароль) из файла при инициализации страницы.
        /// </summary>
        private void LoadCredentialsFromFile()
        {
            if (File.Exists(credentialsFilePath))
            {
                try
                {
                    var lines = File.ReadAllLines(credentialsFilePath);
                    if (lines.Length >= 2)
                    {
                        LoginUser.Text = lines[0];
                        PasswordUser.Password = lines[1];
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка чтения файла с данными:\n{ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Сохранение данных пользователя (логин и пароль) в файл при успешной авторизации.
        /// </summary>
        private void SaveCredentialsToFile()
        {
            try
            {
                var lines = new string[] { LoginUser.Text, PasswordUser.Password };
                File.WriteAllLines(credentialsFilePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка сохранения данных:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке для открытия настроек соединения.
        /// </summary>
        private void OpenConnectionSettings_Cick(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenPage(new PageSettings());
        }

        /// <summary>
        /// Обработчик клика по кнопке авторизации, выполняет проверку и аутентификацию пользователя.
        /// </summary>
        private void Authorization_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на пустой ввод логина и пароля
            if (string.IsNullOrWhiteSpace(LoginUser.Text))
            {
                MessageBox.Show(
                    "Пожалуйста, введите логин.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(PasswordUser.Password))
            {
                MessageBox.Show(
                    "Пожалуйста, введите пароль.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Загрузка настроек из файла (если требуется)
            if (!Settings.LoadSettingsFromFile())
            {
                MessageBox.Show(
                    "Ошибка загрузки настроек.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            using var databaseContext = new DatabaseContext();
            try
            {
                var user = databaseContext.Users.FirstOrDefault(
                    x => x.Login == LoginUser.Text && x.Password == PasswordUser.Password);

                if (user != null)
                {
                    Settings.CurrentUser = user;
                    SaveCredentialsToFile();
                    MessageBox.Show(
                        "Авторизация прошла успешно!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    MainWindow.OpenPage(new MainPage());
                }
                else
                {
                    MessageBox.Show(
                        "Неверный логин или пароль.",
                        "Ошибка авторизации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                UIHelper.ErrorConnection(ex.Message);
                return;
            }
        }
    }
}
