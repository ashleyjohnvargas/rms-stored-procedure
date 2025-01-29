namespace PMS.Models
{
    public class ManagerPaymentViewModel
    {
        public int PaymentId { get; set; }
        public string TenantFullName { get; set; }
        public decimal Amount { get; set; }
        public string UnitName { get; set; }
        public decimal MonthlyRent { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
    }
}
