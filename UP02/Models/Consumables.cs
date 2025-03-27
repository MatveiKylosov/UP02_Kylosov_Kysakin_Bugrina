using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace UP02.Models
{
    public class Consumables
    {
        [Key]
        public int ConsumableID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? ReceiptDate { get; set; }
        [Column(TypeName = "LONGBLOB")]
        public byte[]? Photo { get; set; }
        public int? Quantity { get; set; }
        public int? ResponsibleUserID { get; set; }
        public int? TempResponsibleUserID { get; set; }
        public int? TypeConsumablesID { get; set; }

        public virtual Users? ResponsibleUser { get; set; }
        public virtual Users? TempResponsibleUser { get; set; }
        public virtual TypesConsumables? TypeConsumables { get; set; }

        public string ReceiptDateString => ReceiptDate.HasValue ? ReceiptDate.Value.ToString("dd.MM.yyyy") : "";
    }
}
