using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PMS.Models
{
    public class PropertyManager
    {
        [Key]
        public int ManagerID { get; set; } // Primary Key

        [ForeignKey(nameof(User))]
        public int? UserId { get; set; }
        public string? PhoneNumber { get; set; } // Staff phone number
        public virtual User? User { get; set; }
    }
}
