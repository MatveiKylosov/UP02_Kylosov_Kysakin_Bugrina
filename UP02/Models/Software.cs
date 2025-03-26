using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class Software
    {
        [Key]
        public int SoftwareID { get; set; }
        public string Name { get; set; }
        public int? DeveloperID { get; set; }
        public int? EquipmentID { get; set; }
        public string? Version { get; set; }

        public virtual SoftwareDevelopers? Developer { get; set; }
        public virtual Equipment? Equipment{ get; set; }
    }
}
