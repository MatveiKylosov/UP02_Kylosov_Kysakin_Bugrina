using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class Audiences
    {
        [Key]
        public int AudienceID { get; set; }
        public string Name { get; set; }
        public string? ShortName { get; set; }

        public int? ResponsibleUserID { get; set; }
        public int? TempResponsibleUserID { get; set; }

        public virtual Users? ResponsibleUser { get; set; }
        public virtual Users? TempResponsibleUser { get; set; }
    }
}
