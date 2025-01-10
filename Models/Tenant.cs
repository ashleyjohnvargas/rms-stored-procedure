using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    public class Tenant
    {
        [Key]
        public int TenantID { get; set; } // Primary Key

        [ForeignKey(nameof(User))]
        public int? UserId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePicturePath { get; set; } // File path for the profile picture
        public virtual User? User { get; set; }
    }
}
