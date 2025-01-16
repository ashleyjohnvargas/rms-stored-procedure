using Microsoft.AspNetCore.Mvc;

namespace PMS.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }
    }
}
