using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UP02.Database;
using UP02.Models;

namespace UP02.Context
{
    /// <summary>
    /// Контекст базы данных для работы с Entity Framework.
    /// </summary>
    public class DatabaseContext : DbContext
    {
        public DbSet<ConsumableResponsibleHistory> ConsumableResponsibleHistory { get; set; }

        /// <summary>
        /// Таблица направлений.
        /// </summary>
        public DbSet<Directions> Directions { get; set; }

        /// <summary>
        /// Таблица статусов.
        /// </summary>
        public DbSet<Statuses> Statuses { get; set; }

        /// <summary>
        /// Таблица пользователей.
        /// </summary>
        public DbSet<Users> Users { get; set; }

        /// <summary>
        /// Таблица аудиторий.
        /// </summary>
        public DbSet<Audiences> Audiences { get; set; }

        /// <summary>
        /// Таблица моделей оборудования.
        /// </summary>
        public DbSet<EquipmentModels> EquipmentModels { get; set; }

        /// <summary>
        /// Таблица оборудования.
        /// </summary>
        public DbSet<Equipment> Equipment { get; set; }

        /// <summary>
        /// История местоположений оборудования.
        /// </summary>
        public DbSet<EquipmentLocationHistory> EquipmentLocationHistory { get; set; }

        /// <summary>
        /// История ответственных за оборудование.
        /// </summary>
        public DbSet<EquipmentResponsibleHistory> EquipmentResponsibleHistory { get; set; }

        /// <summary>
        /// Настройки сети.
        /// </summary>
        public DbSet<NetworkSettings> NetworkSettings { get; set; }

        /// <summary>
        /// Типы расходных материалов.
        /// </summary>
        public DbSet<TypesConsumables> TypesConsumables { get; set; }

        /// <summary>
        /// Расходные материалы.
        /// </summary>
        public DbSet<Consumables> Consumables { get; set; }

        /// <summary>
        /// Связь оборудования с расходными материалами.
        /// </summary>
        public DbSet<EquipmentConsumables> EquipmentConsumables { get; set; }

        /// <summary>
        /// Характеристики расходных материалов.
        /// </summary>
        public DbSet<ConsumableCharacteristics> ConsumableCharacteristics { get; set; }

        /// <summary>
        /// Значения характеристик расходных материалов.
        /// </summary>
        public DbSet<ConsumableCharacteristicValues> ConsumableCharacteristicValues { get; set; }

        /// <summary>
        /// Таблица инвентаризаций.
        /// </summary>
        public DbSet<Inventories> Inventories { get; set; }

        /// <summary>
        /// Таблица проверок инвентаризации.
        /// </summary>
        public DbSet<InventoryChecks> InventoryChecks { get; set; }

        /// <summary>
        /// Таблица ошибок.
        /// </summary>
        public DbSet<Errors> Errors { get; set; }

        /// <summary>
        /// Разработчики программного обеспечения.
        /// </summary>
        public DbSet<SoftwareDevelopers> SoftwareDevelopers { get; set; }

        /// <summary>
        /// Таблица программного обеспечения.
        /// </summary>
        public DbSet<Software> Software { get; set; }

        /// <summary>
        /// Таблица типов оборудования.
        /// </summary>
        public DbSet<TypesEquipment> TypesEquipment { get; set; }

        /// <summary>
        /// Конфигурирует параметры подключения к базе данных.
        /// </summary>
        /// <param name="optionsBuilder">Объект для настройки параметров подключения.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseMySql(Settings.ConnectionString, new MySqlServerVersion(new Version(8, 0, 11)), mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,  // Ограничиваем количество попыток переподключения (например, 3 попытки)
                    maxRetryDelay: TimeSpan.FromSeconds(1),  // Ограничиваем максимальную задержку между попытками (например, 5 секунд)
                    errorNumbersToAdd: null))
                .LogTo(message => LogError(message), LogLevel.Error);
        }

        /// <summary>
        /// Записывает ошибки Entity Framework в лог-файл.
        /// </summary>
        /// <param name="message">Текст ошибки.</param>
        private void LogError(string message)
        {
            string logEntry = $"{DateTime.Now}: {message}{Environment.NewLine}";
            File.AppendAllText("ef_errors.log", logEntry);
        }
    }
}
