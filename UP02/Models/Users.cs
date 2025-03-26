using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class Users
    {
        [Key]
        public int UserID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string? Email { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {MiddleName}";
    }
}
