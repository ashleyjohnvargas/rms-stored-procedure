using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PMS.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult SMaintenanceAssignment()
        {
            // Get the logged-in user's ID from the session
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Handle the case where the session does not have a UserId (e.g., redirect to login)
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the staff member associated with the logged-in user
            var staff = _context.Staffs
                .Include(s => s.Requests) // Include requests assigned to the staff
                    .ThenInclude(r => r.Tenant) // Include Tenant for each request
                        .ThenInclude(t => t.User) // Include User for each tenant
                .FirstOrDefault(s => s.UserId == userId);

            if (staff == null)
            {
                // Handle the case where no staff is found (e.g., unauthorized access)
                return Unauthorized();
            }

            // Get all requests assigned to this staff with RequestStatus = "Pending"
            var requests = staff.Requests
                .Where(r => r.RequestStatus != "Completed")
                .ToList();


            // Build the view model to pass to the view
            var model = requests.Select(request =>
            {
                // Get the unit name by matching TenantID in Lease and UnitId in Units
                var lease = _context.Leases
                    .Include(l => l.Unit)
                    .FirstOrDefault(l => l.TenantID == request.TenantID);

                var unitName = lease?.Unit?.UnitName ?? "N/A"; // Default to "N/A" if no unit is found

                // Get tenant's name from the request
                var tenantName = request.Tenant?.User != null
                    ? $"{request.Tenant.User.FirstName} {request.Tenant.User.LastName}"
                    : "N/A"; // Default to "N/A" if tenant or user is null

                return new MaintenanceAssignmentViewModel
                {
                    RequestID = request.RequestID,
                    UnitName = unitName,
                    DateRequested = request.RequestDateTime?.ToString("yyyy-MM-dd hh:mm tt") ?? "N/A",
                    RequestTask = request.RequestDescription ?? "N/A",
                    Category = request.RequestType ?? "N/A",
                    Status = request.RequestStatus ?? "N/A",
                    TenantName = tenantName // Add tenant's name
                };
            }).ToList();

            return View(model);
        }


        public IActionResult SMaintenanceHistory()
        {
            // Get the logged-in user's ID from the session
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Handle the case where the session does not have a UserId (e.g., redirect to login)
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the staff member associated with the logged-in user
            var staff = _context.Staffs
                .Include(s => s.Requests) // Include requests assigned to the staff
                    .ThenInclude(r => r.Tenant) // Include Tenant for each request
                .FirstOrDefault(s => s.UserId == userId);

            if (staff == null)
            {
                // Handle the case where no staff is found (e.g., unauthorized access)
                return Unauthorized();
            }

            // Get all requests assigned to this staff where RequestStatus is not "Pending"
            var requests = staff.Requests
                .Where(r => r.RequestStatus == "Completed")
                .ToList();

            // Build the view model to pass to the view
            var model = requests.Select(request =>
            {
                // Get the unit name by matching TenantID in Lease and UnitId in Units
                var lease = _context.Leases
                    .Include(l => l.Unit)
                    .FirstOrDefault(l => l.TenantID == request.TenantID);

                var unitName = lease?.Unit?.UnitName ?? "N/A"; // Default to "N/A" if no unit is found

                return new MaintenanceHistoryViewModel
                {
                    RequestID = request.RequestID,
                    UnitName = unitName,
                    DateRequested = request.RequestDateTime?.ToString("yyyy-MM-dd hh:mm tt") ?? "N/A",
                    RequestTask = request.RequestDescription ?? "N/A",
                    Category = request.RequestType ?? "N/A",
                    Status = request.RequestStatus ?? "N/A",
                    DateStarted = request.RequestStartDateTime?.ToString("yyyy-MM-dd hh:mm tt") ?? "N/A",
                    DateFinished = request.CompletedDateTime?.ToString("yyyy-MM-dd hh:mm tt") ?? "N/A"
                };
            }).ToList();

            return View(model);
        }


        //[HttpPost]
        public IActionResult StartRequest(int id)
        {
            // Retrieve the request by its ID
            var request = _context.Requests.FirstOrDefault(r => r.RequestID == id);

            if (request == null)
            {
                // Handle the case where the request does not exist
                return NotFound();
            }

            // Update the RequestStartDateTime to the current date and time
            request.RequestStartDateTime = DateTime.Now;

            // Update the RequestStatus to "Ongoing"
            request.RequestStatus = "Ongoing";

            // Save the changes to the database
            _context.SaveChanges();

            TempData["ShowPopup"] = true; // Indicate that the popup should be shown
            TempData["PopupMessage"] = "Request has been successfully started!";
            TempData["PopupTitle"] = "Request Started!";  // Set the custom title
            TempData["PopupIcon"] = "success";  // Set the icon dynamically (can be success, error, info, warning)

            // Redirect to the SMaintenanceAssignment view or wherever is appropriate
            return RedirectToAction("SMaintenanceAssignment");
        }


        [HttpPost]
        public IActionResult CompleteRequest(CompleteRequestViewModel model)
        {
            // Validate that all required fields are provided
            if (model.Date == default || model.Time == null || model.Cost <= 0)
            {
                TempData["ShowPopup"] = true; // Indicate that the popup should be shown
                TempData["PopupMessage"] = "Date, Time, and Cost are required fields.";
                TempData["PopupTitle"] = "Missing required fields!";  // Set the custom title
                TempData["PopupIcon"] = "error";  // Set the icon dynamically (can be success, error, info, warning)
                return RedirectToAction("SMaintenanceAssignment");
            }

            // Retrieve the request from the database using the RequestId
            var request = _context.Requests.FirstOrDefault(r => r.RequestID == model.RequestId);

            if (request == null)
            {
                TempData["Error"] = "The specified request does not exist.";
                return RedirectToAction("SMaintenanceAssignment");
            }

            // Update the CompletedDateTime with the combined Date and Time from the form
            request.CompletedDateTime = model.Date.Add(model.Time);

            // Update the RequestStatus to "Completed"
            request.RequestStatus = "Completed";

            // Update the Cost field of the request
            request.Cost = model.Cost;

            // Retrieve the associated staff using the StaffID from the request
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffID == request.StaffID);

            if (staff != null)
            {
                // Set the IsVacant field of the staff to true
                staff.IsVacant = true;
            }

            // Save the changes to the database
            _context.SaveChanges();

            // Set TempData for success message
            TempData["ShowPopup"] = true; // Indicate that the popup should be shown
            TempData["PopupMessage"] = "Request has been successfully completed.";
            TempData["PopupTitle"] = "Request Completed!";  // Set the custom title
            TempData["PopupIcon"] = "success";  // Set the icon dynamically (can be success, error, info, warning)

            return RedirectToAction("SMaintenanceAssignment");
        }





        public IActionResult SEditProfile()
        {
            return View();
        }

        public IActionResult SHomePage()
        {
            return View();
        }

    }
}
