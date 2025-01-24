using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Models;

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
                .Where(r => r.RequestStatus == "Pending")
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
                .Where(r => r.RequestStatus != "Pending")
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
