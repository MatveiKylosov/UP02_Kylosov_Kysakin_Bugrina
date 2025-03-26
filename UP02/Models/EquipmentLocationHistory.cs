using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UP02.Models
{
    public class EquipmentLocationHistory
    {
        [Key]
        public int HistoryID { get; set; }
        public int EquipmentID { get; set; }
        public DateTime ChangeDate { get; set; }
        public string Comment { get; set; }
        public int? AudienceID { get; set; }

        public virtual Audiences? Audience { get; set; }

        [NotMapped]
        public string ChageDateString => ChangeDate.ToString("HH:mm dd.MM.yyyy", CultureInfo.InvariantCulture);
    }
}
