using Microsoft.AspNetCore.Mvc;

namespace PMS.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            //var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            //if (user != null)
            //{
            //    HttpContext.Session.SetString("UserId", user.Id.ToString());
            //    HttpContext.Session.SetString("UserFullName", user.FullName);
            return RedirectToAction("Index", "Home");
            //}

            //ViewBag.ErrorMessage = "Invalid email or password. Please check your credentials or create an account";
            // View("Login");
        }

        public IActionResult ForgotPass()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}
