using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class EquipmentConsumables
    {
        [Key]
        public int EquipmentConsumableID { get; set; }
        public int EquipmentID { get; set; }
        public int ConsumableID { get; set; }
    }
}
