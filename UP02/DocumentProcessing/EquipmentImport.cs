using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.DocumentProcessing
{
    /// <summary>
    /// Класс для представления информации об импортируемом оборудовании.
    /// </summary>
    public class EquipmentImport
    {
        /// <summary>
        /// Уникальный идентификатор оборудования.
        /// </summary>
        public int ID;

        /// <summary>
        /// Название оборудования.
        /// </summary>
        public string Name;

        /// <summary>
        /// Инвентарный номер оборудования.
        /// </summary>
        public string InventoryNumber;

        /// <summary>
        /// Количество единиц оборудования.
        /// </summary>
        public int Quantity;
    }
}
