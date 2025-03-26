using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class EquipmentModels
    {
        [Key]
        public int ModelID { get; set; }
        public string Name { get; set; }
    }
}
