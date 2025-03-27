using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UP02.Models;

namespace UP02.DocumentProcessing
{
    /// <summary>
    /// Класс для представления информации об импорте пользователей.
    /// </summary>
    public class UserImport
    {
        /// <summary>
        /// Номер строки в документе.
        /// </summary>
        public int Row;

        /// <summary>
        /// Полное имя пользователя.
        /// </summary>
        public string FullName;

        /// <summary>
        /// Количество единиц (например, пользователей или записей).
        /// </summary>
        public int Quantity;

        /// <summary>
        /// Список направлений, связанных с пользователем.
        /// </summary>
        public List<TypesEquipmentImport> TypesEquipments;
    }
}
