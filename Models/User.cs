using System.ComponentModel.DataAnnotations;

namespace PMS.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; } // Primary Key
        public string? FirstName { get; set; } // User's first name
        public string? LastName { get; set; } // User's last name
        public string? Email { get; set; } // User's email
        public string? Password { get; set; } // User's password (hashed)
        public string? Role { get; set; } // Role (Property Manager, Staff, Tenant)
        // Set default value for IsActive in the constructor
        public bool IsActive { get; set; } = true; // Default value is true
    }
}
