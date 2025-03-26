using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class ConsumableCharacteristics
    {
        [Key]
        public int CharacteristicID { get; set; }
        public int TypeConsumablesID { get; set; }
        public string CharacteristicName { get; set; }
    }
}
