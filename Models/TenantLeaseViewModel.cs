namespace PMS.Models
{
    public class TenantLeaseViewModel
    {
        public int? LeaseID { get; set; }
        // Property Information
        public string? PropertyName { get; set; }
        public string? Address { get; set; }

        // Lease Details
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? MonthlyRent { get; set; }
        public string? SecurityDeposit { get; set; }
        public string? Status { get; set; }

        // Tenant Information
        public string? TenantName { get; set; }
        public string? Contact { get; set; }
        public string? Phone { get; set; }
    }
}
