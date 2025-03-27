using OfficeOpenXml;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace UP02.DocumentProcessing
{
    /// <summary>
    /// Класс для импорта данных из Excel.
    /// </summary>
    public class ExcelDataImporter
    {
        private string PathToFile;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelDataImporter"/>.
        /// </summary>
        /// <param name="pathToFile">Путь к файлу Excel.</param>
        public ExcelDataImporter(string pathToFile)
        {
            this.PathToFile = pathToFile;
        }

        /// <summary>
        /// Читает данные из Excel-файла.
        /// </summary>
        /// <returns>Кортеж, содержащий сообщение об ошибке (если есть) и список импортированных пользователей.</returns>
        public (string error, List<UserImport>) ReadData()
        {
            //ToDo: Заменит все на return null на выброс except
            Debug.WriteLine("--- Тест чтения файла Excel ---");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo fileInfo = new FileInfo(PathToFile);
            if (!fileInfo.Exists)
            {
                return ("Файл не найден.", null);
            }

            using var package = new ExcelPackage(fileInfo);
            var worksheet = package.Workbook.Worksheets[0];
            int LastRow = worksheet.Dimension.End.Row + 1;

            bool wrongStruct = false;

            for (int i = 0; i < LastRow; i++)
            {
                if (string.IsNullOrEmpty(worksheet.Cells[$"A{i}"].Text))
                {
                    wrongStruct = true;
                    continue;
                }
            }

            wrongStruct |= !CheckMergedCell(worksheet, "A1", "A1:C1", "сотрудник");
            wrongStruct |= !CheckHeaderRow(worksheet);
            wrongStruct |= !CheckMergedCells(worksheet, "D1", "D2");

            Debug.WriteLine(wrongStruct ? "Неправильная структура!" : "Структура правильная");

            if (wrongStruct)
            {
                Debug.WriteLine("--- Закончил тест чтения файла Excel ---");
                return ("Неправильная структура!", null);
            }


            List<UserImport> UsersRows = GetUsersRow(worksheet, 3, LastRow);

            if (UsersRows.Count == 0)
            {
                return ("Неправильная структура!", null);
            }

            Debug.WriteLine($"\tКоличество пользователей - {UsersRows.Count}");

            for (int i = 0; i < UsersRows.Count; i++)
            {
                var user = UsersRows[i];

                var nextUserRow = i + 1 == UsersRows.Count ? LastRow : UsersRows[i + 1].Row;
                user.TypesEquipments = GetTypesEquipmentsRow(worksheet, user.Row, nextUserRow);
                var TypesEquipments = user.TypesEquipments;
                for (int j = 0; j < TypesEquipments.Count; j++)
                {
                    var TypesEquipment = TypesEquipments[j];
                    var nextTypesEquipmentRow = j + 1 == TypesEquipments.Count ? nextUserRow : TypesEquipments[j + 1].Row;
                    TypesEquipment.Equipments = GetEquipmentRow(worksheet, TypesEquipment.Row, nextTypesEquipmentRow);
                }
            }

            foreach (var user in UsersRows)
            {
                var userQuantity = user.Quantity;
                var TypesEquipmentQuantity = user.TypesEquipments.Select(e => e.Quantity).Sum();
                var equipmentQuantity = user.TypesEquipments.Select(e => e.Equipments.Select(x => x.Quantity).Sum()).Sum();

                if (!(userQuantity == TypesEquipmentQuantity && TypesEquipmentQuantity == equipmentQuantity))
                {
                    Debug.WriteLine("Ошибка: отсутсвует соответсвие по количеству.");
                    Debug.WriteLine("--- Закончил тест чтения файла Excel ---");
                    return ("отсутсвует соответсвие по количеству.", null);
                }
            }

            var countUsers = UsersRows.Count;
            var countTypesEquipment = UsersRows.Select(e => e.TypesEquipments.Count).Sum();
            var countEquipment = UsersRows.Select(e => e.TypesEquipments.Select(f => f.Equipments.Count).Sum()).Sum();

            if (countUsers + countTypesEquipment + countEquipment + 2 != LastRow - 1)
            {
                Debug.WriteLine("Ошибка: количество строк с данными и шапкой не совпадает с концом пустой строки файл.");
                return ("количество строк с данными и шапкой не совпадает с концом пустой строки файл.", null);
            }

            Debug.WriteLine($"Данные успешно обработаны");
            Debug.WriteLine("--- Закончил тест чтения файла Excel ---");

            return ("", UsersRows);
        }

        /// <summary>
        /// Получает список пользователей из таблицы.
        /// </summary>
        private List<UserImport> GetUsersRow(ExcelWorksheet worksheet, int i, int LastRow)
        {
            List<UserImport> UsersRows = new List<UserImport>();

            while (i != LastRow)
            {
                if (CheckUserCell(worksheet, i, out string FullName, out int QuantityUser))
                {
                    var userImport = new UserImport { Row = i, FullName = FullName, Quantity = QuantityUser };
                    UsersRows.Add(userImport);
                }
                i++;
            }

            return UsersRows;
        }

        /// <summary>
        /// Получает список направлений для пользователя.
        /// </summary>
        private List<TypesEquipmentImport> GetTypesEquipmentsRow(ExcelWorksheet worksheet, int UserRow, int EndUserRows)
        {
            List<TypesEquipmentImport> TypesEquipments = new List<TypesEquipmentImport>();

            for (int i = UserRow + 1; i < EndUserRows; i++)
            {
                if (CheckTypesEquipmentCell(worksheet, i, out string TypesEquipmentName, out int Quantity))
                {
                    var TypesEquipment = new TypesEquipmentImport { Row = i, TypesEquipmentName = TypesEquipmentName, Quantity = Quantity };
                    TypesEquipments.Add(TypesEquipment);
                }
            }

            return TypesEquipments;
        }

        /// <summary>
        /// Получает список оборудования для направления.
        /// </summary>
        private List<EquipmentImport> GetEquipmentRow(ExcelWorksheet worksheet, int TypesEquipmentRow, int EndTypesEquipmentRows)
        {
            List<EquipmentImport> equipmentsImport = new List<EquipmentImport>();

            for (int i = TypesEquipmentRow + 1; i < EndTypesEquipmentRows; i++)
            {
                var id = worksheet.Cells[$"A{i}"].Text;
                var name = worksheet.Cells[$"B{i}"].Text;
                var inventoryNumber = worksheet.Cells[$"C{i}"].Text;
                var quaintity = worksheet.Cells[$"D{i}"].Text;

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(inventoryNumber) || string.IsNullOrEmpty(quaintity))
                    continue;

                if (!int.TryParse(id, out int ID) || ID <= 0)
                    continue;

                if (equipmentsImport.Count != 0 && ID != equipmentsImport[equipmentsImport.Count - 1].ID + 1)
                    continue;

                if (!Regex.IsMatch(inventoryNumber, "^[0-9]+$"))
                    continue;

                if (!int.TryParse(quaintity, out int Quaintity) || Quaintity <= 0)
                    continue;

                var equipmentImport = new EquipmentImport { ID = ID, InventoryNumber = inventoryNumber, Name = name, Quantity = Quaintity };

                equipmentsImport.Add(equipmentImport);
            }
            return equipmentsImport;
        }

        /// <summary>
        /// Проверяет корректность данных в ячейке пользователя.
        /// </summary>
        private bool CheckUserCell(ExcelWorksheet worksheet, int i, out string fullName, out int quantity)
        {
            var cellFullName = worksheet.Cells[$"A{i}"];
            if (!cellFullName.Merge)
            {
                fullName = "";
                quantity = -1;
                return false;
            }

            string mergedAddress = worksheet.MergedCells[cellFullName.Start.Row, cellFullName.Start.Column];
            var mergedCells = worksheet.Cells[mergedAddress];

            if (mergedCells.Start.Column != 1 || mergedCells.End.Column != 3)
            {

                fullName = "";
                quantity = -1;
                return false;
            }

            string text = mergedCells.Text.Trim();
            if (!Regex.IsMatch(text, @"^[А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+$"))
            {
                fullName = "";
                quantity = -1;
                return false;
            }

            fullName = text;

            var cellQuantity = worksheet.Cells[$"D{i}"];
            if (!int.TryParse(cellQuantity.Text, out int quan))
            {
                quantity = -1;
                fullName = "";
                return false;
            }
            else if (quan <= 0)
            {
                quantity = -1;
                fullName = "";
                return false;
            }

            quantity = quan;
            return true;
        }

        /// <summary>
        /// Проверяет корректность данных в ячейке направления.
        /// </summary>
        private bool CheckTypesEquipmentCell(ExcelWorksheet worksheet, int i, out string TypesEquipmentName, out int quantity)
        {
            var cellTypesEquipment = worksheet.Cells[$"A{i}"];
            if (!cellTypesEquipment.Merge)
            {
                TypesEquipmentName = "";
                quantity = -1;
                return false;
            }

            string mergedAddress = worksheet.MergedCells[cellTypesEquipment.Start.Row, cellTypesEquipment.Start.Column];
            var mergedCells = worksheet.Cells[mergedAddress];

            if (mergedCells.Start.Column != 1 || mergedCells.End.Column != 3)
            {
                TypesEquipmentName = "";
                quantity = -1;
                return false;
            }

            string text = mergedCells.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                TypesEquipmentName = "";
                quantity = -1;
                return false;
            }

            TypesEquipmentName = text;

            var cellQuantity = worksheet.Cells[$"D{i}"];
            if (!int.TryParse(cellQuantity.Text, out int quan))
            {
                TypesEquipmentName = "";
                quantity = -1;
                return false;
            }
            else if (quan <= 0)
            {
                TypesEquipmentName = "";
                quantity = -1;
                return false;
            }

            quantity = quan;

            return true;
        }

        /// <summary>
        /// Проверяет, объединена ли ячейка и соответствует ли ожидаемым значениям.
        /// </summary>
        private bool CheckMergedCell(ExcelWorksheet worksheet, string cellAddress, string expectedMergeRange, string expectedText)
        {
            var cell = worksheet.Cells[cellAddress];
            if (!cell.Merge || worksheet.MergedCells[cell.Start.Row, cell.Start.Column] != expectedMergeRange)
            {
                Debug.WriteLine($"Ошибка: Ячейка {cellAddress} должна быть объединена в {expectedMergeRange}");
                return false;
            }

            if (!cell.Text.Equals(expectedText, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Ошибка: В ячейке {cellAddress} ожидалось '{expectedText}', а получено '{cell.Text}'");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет заголовок таблицы.
        /// </summary>
        private bool CheckHeaderRow(ExcelWorksheet worksheet)
        {
            var expectedHeaders = new (string Cell, string ExpectedText)[]
            {
            ("A2", "№ п/п"),
            ("B2", "Основное средство"),
            ("C2", "Инвентарный номер")
            };

            foreach (var (cell, expectedText) in expectedHeaders)
            {
                if (worksheet.Cells[cell].Text != expectedText)
                {

                    Debug.WriteLine($"Ошибка: В {cell} ожидалось '{expectedText}', а получено '{worksheet.Cells[cell].Text}'");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Проверяет объединенные ячейки в таблице.
        /// </summary>
        private bool CheckMergedCells(ExcelWorksheet worksheet, string startCell, string endCell)
        {
            var cell = worksheet.Cells[startCell];
            if (!cell.Merge)
            {

                Debug.WriteLine($"Ошибка: Ячейка {startCell} должна быть объединена");
                return false;
            }

            string mergedAddress = worksheet.MergedCells[cell.Start.Row, cell.Start.Column];
            var mergedCells = worksheet.Cells[mergedAddress];

            if (mergedCells.End.Row < worksheet.Cells[endCell].Start.Row)
            {

                Debug.WriteLine($"Ошибка: Ячейки {startCell} и {endCell} должны быть объединены");
                return false;
            }

            return true;
        }
    }
}
