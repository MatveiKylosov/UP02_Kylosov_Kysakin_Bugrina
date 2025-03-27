using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class Inventories
    {
        [Key]
        public int InventoryID { get; set; }
        public DateTime StartDate { get; set; }
        public int UserID { get; set; }
        public DateTime EndDate { get; set; }
        public string Name { get; set; }

        public virtual Users? User { get; set; }
        [NotMapped]
        public string StartDateString => StartDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        [NotMapped]
        public string EndDateString => EndDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
    }
}
