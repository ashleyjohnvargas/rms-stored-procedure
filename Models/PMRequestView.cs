namespace PMS.Models
{
    public class PMRequestView
    {
        public int RequestID { get; set; }
        public string Tenant { get; set; }
        public string Unit { get; set; }
        public string RequestDate { get; set; }
        public string Issue { get; set; }
        public string AssignedStaff { get; set; }
        public string Status { get; set; }
    }
}
