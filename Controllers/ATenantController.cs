using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Models;

namespace PMS.Controllers
{
    public class ATenantController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ATenantController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ATenantHome()
        {
            return View();
        }

        public async Task<IActionResult> ATenantUnits()
        {
            var units = await _context.Units
            .Where(u => u.UnitStatus == "Active") // Filter for Active units
            .Include(u => u.Images)  // Include images
            .ToListAsync();

           

            var unitViewModels = units.Select(u => new PTenantUnitViewModel
            {
                UnitId = u.UnitID,
                UnitName = u.UnitName,
                Description = u.Description,
                NumberOfUnits = u.NumberOfUnits ?? 0,
                NumberOfRooms = u.NumberOfBedrooms ?? 0 + (u.NumberOfBathrooms ?? 0), // Assuming total rooms = bedrooms + bathrooms
                FirstImagePath = u.Images?.FirstOrDefault()?.FilePath ?? "" // Get the first image path
            }).ToList();

            foreach (var unit in unitViewModels)
            {
                Console.WriteLine($"Unit ID from Units list: {unit.UnitId}"); // For debugging
            }


            return View(unitViewModels); // Return the view with the unit list
        }


        [HttpGet]
        public IActionResult ATenantDetails(int id)
        {
            Console.WriteLine("Unit ID of the selected unit: " + id); // For debugging
            // Fetch the unit details based on the provided ID
            var unit = _context.Units
                .Include(u => u.Images) // Include associated images
                .FirstOrDefault(u => u.UnitID == id);

            // Check if the unit exists
            if (unit == null)
            {
                return NotFound(); // Return a 404 error if the unit is not found
            }

            // Prepare the model for the view
            var model = new
            {
                UnitId = unit.UnitID,
                UnitNAME = unit.UnitName,
                Desc = unit.Description,
                NumberOfBedrooms = unit.NumberOfBedrooms ?? 0,
                NumberOfBathrooms = unit.NumberOfBathrooms ?? 0,
                MonthlyRent = unit.PricePerMonth.HasValue ? unit.PricePerMonth.Value : 0,
                PropertyAddress = $"{unit.Location} {unit.Town} {unit.City} {unit.State} {unit.Country} {unit.ZipCode}",
                MainImage = unit.Images?.FirstOrDefault()?.FilePath, // First image for main display
                GalleryImages = unit.Images?.Skip(1).Select(i => i.FilePath).ToList() // Other images for the gallery
            };
            Console.WriteLine("Unit ID to be passed to Book This Unit Now: " + model.UnitId); // For debugging

            return View(model); // Render the PTenantDetails view with the model
        }

        public IActionResult ATenantApply(int unitId)
        {
            Console.WriteLine($"unitId received: {unitId}"); // For debugging
            return View(unitId);
        }

        [HttpPost]
        public IActionResult ApplyForLease(LeaseApplication leaseApplication)
        {
            // Log all values of the LeaseApplication object to the console
            Console.WriteLine("LeaseApplication object values:");
            Console.WriteLine($"FullName: {leaseApplication.FullName}");
            Console.WriteLine($"BirthDate: {leaseApplication.BirthDate}");
            Console.WriteLine($"ContactNumber: {leaseApplication.ContactNumber}");
            Console.WriteLine($"Email: {leaseApplication.Email}");
            Console.WriteLine($"CurrentAddress: {leaseApplication.CurrentAddress}");
            Console.WriteLine($"EmploymentStatus: {leaseApplication.EmploymentStatus}");
            Console.WriteLine($"EmployerName: {leaseApplication.EmployerName}");
            Console.WriteLine($"MonthlyIncome: {leaseApplication.MonthlyIncome}");
            Console.WriteLine($"UnitId: {leaseApplication.UnitId}");
            Console.WriteLine($"LeaseStartDate: {leaseApplication.LeaseStartDate}");
            Console.WriteLine($"LeaseDuration: {leaseApplication.LeaseDuration}");
            Console.WriteLine($"TermsAndConditions: {leaseApplication.TermsAndConditions}");
            if (leaseApplication == null)
            {
                return BadRequest("Invalid lease application.");
            }

            

            // Validate required fields
            if (leaseApplication.UnitId == null ||
                leaseApplication.LeaseStartDate == null || leaseApplication.LeaseDuration == null ||
                leaseApplication.TermsAndConditions != true)
            {
                ModelState.AddModelError("", "All required fields must be filled out.");
                return RedirectToAction("ATenantApply", new { unitId = leaseApplication.UnitId });
            }

            // Retrieve UserId from session
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in.");
            }

            // Retrieve TenantId from the Tenants table based on UserId
            var tenant = _context.Tenants.FirstOrDefault(t => t.UserId == userId);
            if (tenant == null)
            {
                return BadRequest("Tenant not found for the logged-in user.");
            }

            // Calculate LeaseEndDate based on LeaseDuration and LeaseStartDate
            DateTime? leaseEndDate = leaseApplication.LeaseStartDate?.AddMonths(leaseApplication.LeaseDuration.Value);

            // Create Lease instance
            var lease = new Lease
            {
                TenantID = tenant.TenantID,
                UnitId = leaseApplication.UnitId,
                LeaseStartDate = leaseApplication.LeaseStartDate,
                LeaseDuration = leaseApplication.LeaseDuration,
                LeaseEndDate = leaseEndDate,
                LeaseStatus = "Pending", // Default value for LeaseStatus
                TermsAndConditions = leaseApplication.TermsAndConditions
            };
            _context.Leases.Add(lease);
            _context.SaveChanges();

            // Create LeaseDetails instance
            var leaseDetails = new LeaseDetails
            {
                LeaseID = lease.LeaseID,
                FullName = leaseApplication.FullName,
                BirthDate = leaseApplication.BirthDate,
                ContactNumber = leaseApplication.ContactNumber,
                Email = leaseApplication.Email,
                CurrentAddress = leaseApplication.CurrentAddress,
                EmploymentStatus = leaseApplication.EmploymentStatus,
                EmployerName = leaseApplication.EmployerName,
                MonthlyIncome = leaseApplication.MonthlyIncome
            };

            _context.LeaseDetails.Add(leaseDetails);
            _context.SaveChanges();

            try
            {
                TempData["ShowPopup"] = true; // Indicate that the popup should be shown
                TempData["PopupMessage"] = "Your application has been sent successfully!";
                return RedirectToAction("ATenantHome"); // Redirect to a success page or confirmation
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the lease application: " + ex.Message);
                return RedirectToAction("ATenantApply", new { unitId = leaseApplication.UnitId });
            }
        }



        public IActionResult ATenantLease()
        {
            // Fetch the logged-in user's ID from the session
            var userId = HttpContext.Session.GetInt32("UserId");
            //if (string.IsNullOrEmpty(userId))
            //{
            //    return RedirectToAction("Login", "Account"); // Redirect to login if no user is logged in
            //}

            // Get the tenant's details based on the UserId
            var tenant = _context.Tenants.FirstOrDefault(t => t.UserId == userId);
            if (tenant == null)
            {
                return NotFound("Tenant not found.");
            }

            // Get the lease details for the tenant
            var lease = _context.Leases
                .Include(l => l.Unit) // Include Unit details for property information
                .Include(l => l.LeaseDetails) // Include LeaseDetails for tenant info
                .FirstOrDefault(l => l.TenantID == tenant.TenantID);

            if (lease == null)
            {
                return NotFound("Lease not found.");
            }

            // Get the current date
            var currentDate = DateTime.Now;

            // Calculate the first day of the next month
            var firstDayNextMonth = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1);

            // Construct the model for the view
            var model = new TenantLeaseViewModel
            {
                LeaseID = lease.LeaseID,
                PropertyName = lease.Unit.UnitName, // Assuming UnitName contains the property name
                Address = $"{lease.Unit.Location} {lease.Unit.City} {lease.Unit.Town} {lease.Unit.State},{lease.Unit.ZipCode} {lease.Unit.Country}",
                StartDate = lease.LeaseStartDate?.ToString("MMMM d, yyyy") ?? "N/A",
                EndDate = lease.LeaseEndDate?.ToString("MMMM d, yyyy") ?? "N/A",
                MonthlyRent = string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-PH"), "{0:C}", lease.Unit.PricePerMonth),
                SecurityDeposit = string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-PH"), "{0:C}", lease.Unit.SecurityDeposit),
                Status = lease.LeaseStatus,
                TenantName = lease.LeaseDetails.FullName,
                Contact = lease.LeaseDetails.Email,
                Phone = lease.LeaseDetails.ContactNumber,
                DueDate = firstDayNextMonth.ToString("MMMM d, yyyy") 
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult PayLease(PaymentViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);  // Return the same view with validation errors
            //}

            // Fetch the lease details based on LeaseId
            var lease = _context.Leases.FirstOrDefault(l => l.LeaseID == model.LeaseId);
            if (lease == null)
            {
                return NotFound("Lease not found.");
            }

            // Create a new instance of the Payments table
            var payment = new Payment
            {
                LeaseID = lease.LeaseID,                // LeaseId from the form
                PaymentDate = DateTime.Now,              // Current date and time for PaymentDate
                Amount = model.Amount,                   // Amount from the form
                PaymentMethod = model.PaymentMethod,     // Selected PaymentMethod from the form
                PaymentStatus = "Paid"                   // Default PaymentStatus as "Paid"
            };

            // Save the new payment record to the database
            _context.Payments.Add(payment);
            _context.SaveChanges();

            TempData["ShowPopup"] = true; // Indicate that the popup should be shown
            TempData["PopupMessage"] = "Your payment has been sent successfully!";

            // Redirect to the appropriate page after successful payment
            return RedirectToAction("ATenantLease");
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
