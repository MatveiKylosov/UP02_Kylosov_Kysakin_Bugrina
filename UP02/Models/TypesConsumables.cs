using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class TypesConsumables
    {
        [Key]
        public int TypeConsumablesID { get; set; }
        public string Type { get; set; }
    }
}
