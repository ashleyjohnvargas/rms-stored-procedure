namespace PMS.Models
{
    public class DashboardMetrics
    {
        public int TotalUnits { get; set; }
        public int TotalTenants { get; set; }
        public int UnitsAvailable { get; set; }
        public decimal TotalIncomeToday { get; set; }
        public decimal TotalIncomeThisMonth { get; set; }
        public decimal OccupancyRate { get; set; }
    }
}
