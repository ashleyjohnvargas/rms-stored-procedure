using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PMS.Models
{
    public class MaintenanceRequest
    {
        [Key]
        public int RequestID { get; set; } // Primary Key

        [ForeignKey(nameof(Unit))]
        public int? UnitID { get; set; } // Foreign Key referencing Unit

        [ForeignKey(nameof(Tenant))]
        public int? TenantID { get; set; } // Foreign Key referencing Tenant

        [ForeignKey(nameof(Staff))]
        public int? StaffID { get; set; } // Nullable, as a staff might not be assigned immediately
        public DateTime? RequestDate { get; set; } // Date when the request was made

        public string? Description { get; set; } // Description of the maintenance issue
        public string RequestStatus { get; set; } = "Pending"; // Default value is "Active" // Status as string (e.g., "Pending", "InProgress", "Resolved")

        public DateTime? ResolutionDate { get; set; } // Nullable, only set when resolved

        // Navigation Properties
        public virtual Unit? Unit { get; set; } // Navigation property for Unit
        public virtual Tenant? Tenant { get; set; } // Navigation property for Tenant
        public virtual Staff? Staff { get; set; } // Navigation property for Staff (if assigned)s
    }
}
