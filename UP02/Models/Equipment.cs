using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Models
{
    public class Equipment
    {
        [Key]
        public int EquipmentID { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "LONGBLOB")]
        public byte[]? Photo { get; set; }

        public string InventoryNumber { get; set; }
        public int? ResponsibleUserID { get; set; }
        public int? TempResponsibleUserID { get; set; }
        public decimal? Cost { get; set; }
        public int? DirectionID { get; set; }
        public int? StatusID { get; set; }
        public int? ModelID { get; set; }
        public string? Comment { get; set; }
        public int? AudienceID { get; set; }
        public int? TypeEquipmentID { get; set; }

        public virtual Users? ResponsibleUser { get; set; }
        public virtual Users? TempResponsibleUser { get; set; }
        public virtual Directions? Direction { get; set; }
        public virtual Statuses? Status { get; set; }
        public virtual EquipmentModels? Model { get; set; }
        public virtual Audiences? Audience { get; set; }
        public virtual TypesEquipment? TypeEquipment { get; set; }
    }
}
