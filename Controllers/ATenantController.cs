using Microsoft.AspNetCore.Mvc;

namespace PMS.Controllers
{
    public class ATenantController : Controller
    {
        public IActionResult ATenantHome()
        {
            return View();
        }
        public IActionResult ATenantLease()
        {
            return View();
        }
        public IActionResult ATenantPayment()
        {
            return View();
        }
        public IActionResult ATenantMaintenance()
        {
            return View();
        }
        public IActionResult ATenantEditProfile()
        {
            return View();
        }
    }
}
