using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using UP02.Models;


namespace UP02.Database
{

    /// <summary>
    /// Класс для хранения и управления настройками приложения.
    /// </summary>
    static public class Settings
    {
        /// <summary>
        /// Имя файла настроек.
        /// </summary>
        private const string SettingsFile = "settings.json";

        /// <summary>
        /// IP-адрес сервера базы данных.
        /// </summary>
        public static string IPAddress { get; set; } = string.Empty;

        /// <summary>
        /// Порт подключения к базе данных.
        /// </summary>
        public static string Port { get; set; } = string.Empty;

        /// <summary>
        /// Логин пользователя базы данных.
        /// </summary>
        public static string Login { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя базы данных.
        /// </summary>
        public static string Password { get; set; } = string.Empty;

        /// <summary>
        /// Текущий авторизованный пользователь.
        /// </summary>
        public static Users CurrentUser { get; set; }

        /// <summary>
        /// Строка подключения к базе данных, сформированная на основе текущих настроек.
        /// </summary>
        public static string ConnectionString =>
            $"server={IPAddress};port={Port};user={Login};password={Password};database=EquipmentDB;Connection Timeout=2;Pooling=false;";

        /// <summary>
        /// Сохраняет текущие настройки в файл.
        /// </summary>
        public static void SaveSettingsToFile()
        {
            try
            {
                var settings = new AppSettings
                {
                    IPAddress = IPAddress,
                    Port = Port,
                    Login = Login,
                    Password = Password
                };

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает настройки из файла.
        /// </summary>
        /// <returns>Возвращает true, если загрузка успешна, иначе false.</returns>
        public static bool LoadSettingsFromFile()
        {
            try
            {
                if (!File.Exists(SettingsFile))
                    return false;

                var json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (settings != null)
                {
                    IPAddress = settings.IPAddress ?? string.Empty;
                    Port = settings.Port ?? string.Empty;
                    Login = settings.Login ?? string.Empty;
                    Password = settings.Password ?? string.Empty;

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Внутренний класс для хранения структуры настроек.
        /// </summary>
        private class AppSettings
        {
            /// <summary>
            /// IP-адрес сервера базы данных.
            /// </summary>
            public string IPAddress { get; set; } = string.Empty;

            /// <summary>
            /// Порт подключения к базе данных.
            /// </summary>
            public string Port { get; set; } = string.Empty;

            /// <summary>
            /// Логин пользователя базы данных.
            /// </summary>
            public string Login { get; set; } = string.Empty;

            /// <summary>
            /// Пароль пользователя базы данных.
            /// </summary>
            public string Password { get; set; } = string.Empty;
        }
    }

}
