using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class TypesEquipment
    {
        [Key]
        public int TypeEquipmentID { get; set; }
        public string Name { get; set; }
    }
}
