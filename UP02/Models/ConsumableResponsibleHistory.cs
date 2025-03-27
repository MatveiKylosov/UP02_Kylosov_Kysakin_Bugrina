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
    public class ConsumableResponsibleHistory
    {
        [Key]
        public int HistoryID { get; set; }
        public int ConsumableID {  get; set; }
        public int? OldUserID { get; set; }
        public DateTime ChangeDate {  get; set; }
        public virtual Users? OldUser { get; set; }

        [NotMapped]
        public string ChageDateString => ChangeDate.ToString("HH:mm dd.MM.yyyy", CultureInfo.InvariantCulture);
    }
}
