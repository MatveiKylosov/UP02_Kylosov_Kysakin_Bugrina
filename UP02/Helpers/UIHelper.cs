using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Text.RegularExpressions;
using UP02.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using UP02.Interfaces;
using System.Runtime.CompilerServices;
using UP02.Pages.Main;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using UP02.Context;

namespace UP02.Helpers
{
    public static class UIHelper
    {
        /// <summary>
        /// Валидирует поле ввода.
        /// </summary>
        /// <param name="fieldValue">Значение поля.</param>
        /// <param name="maxLength">Максимальная длина.</param>
        /// <param name="fieldName">Название поля.</param>
        /// <param name="regexPattern">Регулярное выражение (опционально).</param>
        /// <param name="isRequired">Обязательное ли поле.</param>
        /// <returns>Возвращает true, если обнаружена ошибка.</returns>
        public static bool ValidateField(
            string fieldValue,
            int maxLength,
            string fieldName,
            string regexPattern = null,
            bool isRequired = false)
        {
            // Если поле обязательно для заполнения, проверяем, что оно не пустое или не null.
            if (isRequired && string.IsNullOrWhiteSpace(fieldValue))
            {
                MessageBox.Show(
                    $"Поле \"{fieldName}\" обязательно для заполнения.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return true;
            }

            // Если поле не обязательное и значение пустое, дальнейшие проверки пропускаем.
            if (string.IsNullOrWhiteSpace(fieldValue))
            {
                return false;
            }

            // Проверка длины
            if (fieldValue.Length > maxLength)
            {
                MessageBox.Show(
                    $"Поле \"{fieldName}\" не должно быть больше {maxLength} символов.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return true;
            }

            // Проверка соответствия формату, если передан шаблон регулярного выражения
            if (!string.IsNullOrEmpty(regexPattern) && !Regex.IsMatch(fieldValue, regexPattern))
            {
                string expectedExample = GetRegexExample(regexPattern);
                MessageBox.Show(
                    $"Поле \"{fieldName}\" имеет неверный формат. Ожидаемый формат: {expectedExample}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Возвращает пример соответствующего формату значения.
        /// </summary>
        /// <param name="regexPattern">Шаблон регулярного выражения.</param>
        /// <returns>Пример значения.</returns>
        private static string GetRegexExample(string regexPattern)
        {
            if (regexPattern == @"^[a-zA-Z0-9_]+$")
                return "например, user_123";
            if (regexPattern == @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d!@#$%^&*()_+\-=]{8,}$")
                return "например, Pass1234";
            if (regexPattern == @"^[\w.-]+@[a-zA-Z\d.-]+\.[a-zA-Z]{2,6}$")
                return "например, example@mail.com";
            if (regexPattern == @"^[А-Яа-яЁёA-Za-z\-]+$")
                return "например, Иван-Иванов";
            if (regexPattern == @"^\+7\d{10}$")
                return "например, +71234567890";

            // Если шаблон не распознан, можно вернуть сам шаблон или общее сообщение.
            return regexPattern;
        }


        /// <summary>
        /// Универсальный метод для установки выбранного элемента из ComboBox в обновляемый объект.
        /// </summary>
        /// <typeparam name="TEntity">Тип сущности (например, Users, Directions и т.д.)</typeparam>
        /// <typeparam name="TKey">Тип идентификатора сущности</typeparam>
        /// <param name="comboBox">ComboBox, из которого берём выбранный элемент</param>
        /// <param name="setId">Делегат для установки идентификатора в объекте обновления</param>
        /// <param name="setEntity">Делегат для установки самой сущности в объекте обновления</param>
        /// <param name="getId">Делегат для получения идентификатора из сущности</param>
        /// <param name="invalidId">Значение недопустимого идентификатора (например, -1)</param>
        /// <param name="dbSet">Контекст базы данных для привязки сущности</param>
        public static void SetEntity<TEntity, TKey>(
            ComboBox comboBox,
            Action<TKey?> setId,
            Action<TEntity> setEntity,
            Func<TEntity, TKey> getId,
            TKey invalidId,
            DbSet<TEntity> dbSet)
            where TEntity : class
        {
            var entity = comboBox.SelectedItem as TEntity;
            if (entity != null && !getId(entity).Equals(invalidId))
            {
                setId(getId(entity));
                dbSet.Attach(entity);
                setEntity(entity);
            }
            else
            {
                setId(default(TKey?));
                setEntity(null);
            }
        }

        /// <summary>
        /// Добавляет элементы в StackPanel и обрабатывает удаление.
        /// </summary>
        public static void AddItemsToPanel<T>(
            StackPanel panel,
            IEnumerable<T> items,
            Func<T, UIElement> itemCreator,
            List<T> OriginalsItems,
            EventHandler RecordUpdate
            )
        {
            foreach (var item in items)
            {
                var element = itemCreator(item);

                if (element is IRecordDeletable recordDeletable)
                {
                    recordDeletable.RecordDelete += (sender, e) =>
                    {
                        OriginalsItems.Remove(item);
                        if (sender is UIElement element && panel.Children.Contains(element))
                        {
                            panel.Children.Remove(element);
                        }
                    };

                    if (element is IRecordUpdatable recordUpdatable)
                    {
                        recordUpdatable.RecordUpdate += RecordUpdate;
                    }

                    panel.Children.Add(element);
                }
            }
        }

        /// <summary>
        /// Обрабатывает ошибку подключения к базе данных.
        /// </summary>
        /// <param name="exMessage">Сообщение об ошибке.</param>
        public static void ErrorConnection(string exMessage)
        {
            bool WrongConnections = false;
            try
            {
                using var databaseContext = new DatabaseContext();
                var errorEntry = new Errors
                {
                    ErrorTime = DateTime.Now,
                    ErrorMessage = exMessage,
                };
                databaseContext.Errors.Add(errorEntry);
                databaseContext.SaveChanges();
                WrongConnections = false;
            }
            catch (Exception exs)
            {
                WrongConnections = true;
                string logEntry = $"{DateTime.Now}: {exMessage}{Environment.NewLine}\tОшибка при обращении к бд: {exs}";
                File.AppendAllText("ef_errors.log", logEntry);
            }

            if (WrongConnections) {
                MainWindow.ClearFrame();
                MainWindow.OpenPage(new PageAuthorization());
            }

            MessageBox.Show(WrongConnections ? "Не удалось подключиться к базе данных. Проверьте соединение и повторите попытку." : "произошла непредвиденная ошибка.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Преобразует массив байтов в ImageSource для использования в WPF.
        /// </summary>
        /// <param name="imageData">Массив байтов с данными изображения.</param>
        /// <returns>Объект ImageSource, готовый для отображения.</returns>
        public static ImageSource ByteArrayToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            try
            {
                using (var ms = new MemoryStream(imageData))
                {
                    ms.Position = 0;
                    image.BeginInit();
                    // Загружаем изображение сразу в память, чтобы поток можно было закрыть
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                }
                image.Freeze(); // Для безопасного использования в других потоках
            }
            catch
            {
                image = null;
            }
            return image;
        }
    }
}
