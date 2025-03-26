using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class ConsumableCharacteristicValues
    {
        [Key]
        public int CharacteristicsValueID { get; set; }
        public int? CharacteristicID { get; set; }
        public int? ConsumablesID { get; set; }
        public string? Value { get; set; }
    }
}
