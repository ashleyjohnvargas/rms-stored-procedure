using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Models;

namespace PMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

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
            var user = new User();
            return View(user);
        }

        // RegisterUser action to handle form submission and save user data
        [HttpPost]
        public IActionResult RegisterUser(User user)
        {
            // Check if passwords match
            if (user.Password != user.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password and Confirm Password do not match.");
                return RedirectToAction("Register"); // Redirect to the Register action
            }

            // Check if Terms and Conditions are accepted
            if (user.TermsAndConditions.HasValue && !user.TermsAndConditions.Value)
            {
                ModelState.AddModelError("", "You must agree to the terms and conditions.");
                return RedirectToAction("Register"); // Redirect to the Register action
            }

            // Set the Role based on the email domain
            if (user.Email.Contains("@manager"))
            {
                user.Role = "Property Manager";
            }
            else if (user.Email.Contains("@staff"))
            {
                user.Role = "Staff";
            }
            else
            {
                user.Role = "Tenant"; // Default role if the email doesn't match above criteria
            }


            // Create a new User object and populate it with data
            var newUser = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                Role = user.Role, // Default or set role based on your requirement
                TermsAndConditions = user.TermsAndConditions,
            };

            // Save the new user to the database
            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Redirect based on the user's role
            if (user.Role == "Property Manager")
            {
                return RedirectToAction("PMDashboard", "PropertyManager");
            }
            else if (user.Role == "Staff")
            {
                return RedirectToAction("SMaintenanceAssignment", "Staff");
            }
            else if (user.Role == "Tenant")
            {
                return RedirectToAction("PTenantHomePage", "PTenant");
            }

            // If none of the conditions match, return to the Register action (or another appropriate action)
            return RedirectToAction("Register"); // This is the fallback action, should never be reached

        }
    }
}
