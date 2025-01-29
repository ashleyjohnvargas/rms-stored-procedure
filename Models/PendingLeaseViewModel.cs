﻿namespace PMS.Models
{
    public class PendingLeaseViewModel
    {
        public int LeaseID { get; set; }
        public string TenantName { get; set; }
        public string Email { get; set; }
        public string UnitName { get; set; }
        public decimal MonthlyRent { get; set; }
        public DateTime LeaseStartDate { get; set; }
        public DateTime LeaseEndDate { get; set; }
        public string LeaseStatus { get; set; }
    }
}
