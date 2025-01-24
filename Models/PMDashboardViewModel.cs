namespace PMS.Models
{
    public class PMDashboardViewModel
    {
        public int TotalUnits { get; set; }
        public int TotalTenants { get; set; }
        public double OccupancyRate { get; set; }
        public int UnitsAvailable { get; set; }
        public decimal TotalIncomeToday { get; set; }
        public decimal TotalIncomeThisMonth { get; set; }
        public List<MaintenanceStatusViewModel> MaintenanceStatus { get; set; }
        public List<TopRentedUnitViewModel> TopRentedUnits { get; set; }
    }
}
