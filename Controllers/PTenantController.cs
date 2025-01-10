using Microsoft.AspNetCore.Mvc;

namespace PMS.Controllers
{
    public class PTenantController : Controller
    {
        public IActionResult PTenantHomePage()
        {
            return View();
        }

        public IActionResult PTenantUnits()
        {
            return View();
        }

        public IActionResult PTenantDetails()
        {
            return View();
        }
        public IActionResult PTenantApply()
        {
            return View();
        }
    }
}
