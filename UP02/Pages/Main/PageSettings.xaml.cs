using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using UP02.Context;
using UP02.Database;

namespace UP02.Pages.Main
{
    /// <summary>
    /// Логика взаимодействия для PageSettings.xaml
    /// </summary>
    public partial class PageSettings : Page
    {
        /// <summary>
        /// Конструктор страницы настроек. Инициализирует компоненты и загружает настройки из файла, если они существуют.
        /// </summary>
        public PageSettings()
        {
            InitializeComponent();
            if (Settings.LoadSettingsFromFile())
            {
                IPAddress.Text = Settings.IPAddress;
                Port.Text = Settings.Port;
                Login.Text = Settings.Login;
                Password.Password = Settings.Password;
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке "Назад". Возвращает пользователя на предыдущую страницу.
        /// </summary>
        private void ReturnToLogin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GoBack();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Применить настройки". Проверяет корректность введенных данных (IP-адрес, порт, логин, пароль),
        /// сохраняет их в настройки и проверяет подключение к базе данных. Выводит сообщение о результате подключения.
        /// </summary>
        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            var ipAddress = IPAddress.Text;
            string pattern = @"^(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
               + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
               + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)\."
               + @"(25[0-5]|2[0-4][0-9]|1?[0-9][0-9]?)$";

            if (!Regex.IsMatch(ipAddress, pattern))
            {
                MessageBox.Show("Некорректный IP-адрес!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Port.Text) || !Regex.IsMatch(Port.Text, @"^([1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$"))
            {
                MessageBox.Show("Введите корректный порт (1-65535)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                MessageBox.Show("Поле 'Логин' не должно быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Password.Password))
            {
                MessageBox.Show("Поле 'Пароль' не должно быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Settings.IPAddress = ipAddress;
            Settings.Port = Port.Text;
            Settings.Login = Login.Text;
            Settings.Password = Password.Password;


            using (var databaseContext = new DatabaseContext())
            {
                if (databaseContext.Database.CanConnect())
                {
                    Settings.SaveSettingsToFile();
                    MessageBox.Show("Подключение к базе данных успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    MainWindow.GoBack();
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
