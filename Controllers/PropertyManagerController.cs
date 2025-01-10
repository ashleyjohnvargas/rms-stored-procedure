using Microsoft.AspNetCore.Mvc;

namespace PMS.Controllers
{
    public class PropertyManagerController : Controller
    {
        public IActionResult PMDashboard()
        {
            return View();
        }
        public IActionResult PMManageLease()
        {
            return View();
        }
        public IActionResult PMAssignMaintenance()
        {
            return View();
        }

        public IActionResult PMManageUnits()
        {
            return View();
        }
        public IActionResult PMPayments()
        {
            return View();
        }

        public IActionResult PMRequest()
        {
            return View();
        }
        public IActionResult PMTenants()
        {
            return View();
        }
    }
}
