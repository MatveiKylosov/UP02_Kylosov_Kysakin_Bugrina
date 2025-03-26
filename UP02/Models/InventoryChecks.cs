using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class InventoryChecks
    {
        [Key]
        public int CheckID { get; set; }
        public int InventoryID { get; set; }
        public int EquipmentID { get; set; }
        public DateTime CheckDate { get; set; }
        public string Comment { get; set; }

        public int? UserID { get; set; }
        public virtual Users? User { get; set; }

    }
}
