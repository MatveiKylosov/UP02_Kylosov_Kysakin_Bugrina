using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.DocumentProcessing
{
    /// <summary>
    /// Класс для представления информации об импорте направлений.
    /// </summary>
    public class DirectionImport
    {
        /// <summary>
        /// Номер строки в документе.
        /// </summary>
        public int Row;

        /// <summary>
        /// Название направления.
        /// </summary>
        public string DirectionName;

        /// <summary>
        /// Количество единиц данного направления.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// Список импортируемого оборудования в рамках направления.
        /// </summary>
        public List<EquipmentImport> Equipments;
    }
}