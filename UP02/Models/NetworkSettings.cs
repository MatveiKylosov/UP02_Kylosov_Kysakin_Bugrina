using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class NetworkSettings
    {
        [Key]
        public int NetworkID { get; set; }
        public int? EquipmentID { get; set; }
        public string IPAddress { get; set; }
        public string SubnetMask { get; set; }
        public string Gateway { get; set; }
        public string DNSServers { get; set; }

        public virtual Equipment? Equipment { get; set; }
    }
}
