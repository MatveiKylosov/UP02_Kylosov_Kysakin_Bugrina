using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class Errors
    {
        [Key]
        public int ErrorID { get; set; }
        public DateTime ErrorTime { get; set; }
        public string ErrorMessage { get; set; }
    }
}
