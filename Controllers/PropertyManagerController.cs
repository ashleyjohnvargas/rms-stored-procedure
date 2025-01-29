﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Models;
using Microsoft.Data.SqlClient;


namespace PMS.Controllers
{
    public class PropertyManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PropertyManagerController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> PMDashboard()
        {
            // Total Units: Count all active units
            var totalUnits = await _context.Units
                .CountAsync(u => u.UnitStatus == "Active");

            // Total Tenants: Count all users with Role "Tenant" and IsActive = true
            var totalTenants = await _context.Users
                .CountAsync(u => u.Role == "Tenant" && u.IsActive);

            // Units Available: Count all units with AvailabilityStatus "Available"
            var unitsAvailable = await _context.Units
                .CountAsync(u => u.AvailabilityStatus == "Available");

            // Total Income Today: Sum of Amount for today's payments
            var totalIncomeToday = await _context.Payments
                .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value.Date == DateTime.Now.Date)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Total Income This Month: Sum of Amount for payments in the current month
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var totalIncomeThisMonth = await _context.Payments
                .Where(p => p.PaymentDate.HasValue &&
                            p.PaymentDate.Value.Month == currentMonth &&
                            p.PaymentDate.Value.Year == currentYear)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Occupancy Rate: Percentage of active units that are leased
            var totalLeasedUnits = await _context.Leases
                .Select(l => l.UnitId)
                .Distinct()
                .CountAsync();
            var occupancyRate = totalUnits > 0 ? (double)totalLeasedUnits / totalUnits * 100 : 0;

            // Maintenance Status: Calculate the percentage of each request status
            var totalRequests = await _context.Requests.CountAsync();
            var maintenanceStatus = await _context.Requests
                .GroupBy(r => r.RequestStatus)
                .Select(group => new MaintenanceStatusViewModel
                {
                    Status = group.Key,
                    Percentage = totalRequests > 0 ? (group.Count() * 100.0) / totalRequests : 0
                })
                .ToListAsync();


            // Top Rented Units: Frequency of each unit in the Leases table (percentage)
            var topRentedUnits = await _context.Leases
                .GroupBy(l => l.Unit.UnitName)
                .Select(group => new TopRentedUnitViewModel
                {
                    UnitName = group.Key,
                    Percentage = totalLeasedUnits > 0 ? (group.Count() * 100.0) / totalLeasedUnits : 0
                })
                .OrderByDescending(u => u.Percentage)
                .Take(5) // Limit to top 5
                .ToListAsync();


            // Prepare the dashboard view model
            var dashboardModel = new PMDashboardViewModel
            {
                TotalUnits = totalUnits,
                TotalTenants = totalTenants,
                OccupancyRate = Math.Round(occupancyRate, 2),
                UnitsAvailable = unitsAvailable,
                TotalIncomeToday = totalIncomeToday,
                TotalIncomeThisMonth = totalIncomeThisMonth,
                MaintenanceStatus = maintenanceStatus,
                TopRentedUnits = topRentedUnits
            };

            return View(dashboardModel);
        }


        public IActionResult PMManageLease()
        {
            // Fetch data from the view using the DbContext
            var leases = _context.PendingLeasesView
                .Select(v => new LeaseViewModel
                {
                    LeaseID = v.LeaseID,
                    TenantName = v.TenantName,
                    Email = v.Email,
                    UnitName = v.UnitName,
                    MonthlyRent = v.MonthlyRent,
                    LeaseStartDate = v.LeaseStartDate,
                    LeaseEndDate = v.LeaseEndDate,
                    LeaseStatus = v.LeaseStatus
                })
                .ToList();

            return View(leases);
        }


        public IActionResult PMActiveLease()
        {
            var leases = _context.LeaseView.FromSqlRaw("SELECT * FROM VW_PMActiveLease").ToList();

            return View(leases);
        }



        public IActionResult DownloadLeaseAgreement(int id)
        {
            // Retrieve the lease using the LeaseID
            var lease = _context.Leases.FirstOrDefault(l => l.LeaseID == id);

            if (lease == null || string.IsNullOrEmpty(lease.LeaseAgreementFilePath))
            {
                // If no lease found or file path is missing, return an error message
                TempData["ErrorMessage"] = "Lease agreement not found.";
                return RedirectToAction("PMManageLease"); // Redirect to a relevant page
            }

            // Combine the base path (wwwroot) and the relative file path stored in the database
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", lease.LeaseAgreementFilePath.TrimStart('/'));

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "Lease agreement file not found.";
                return RedirectToAction("PMManageLease"); // Redirect to a relevant page
            }

            // Get the file content
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Return the file for download with appropriate content type and filename
            return File(fileBytes, "application/pdf", Path.GetFileName(filePath));
        }



        public IActionResult PMAssignMaintenance()
        {
            return View();
        }

        public async Task<IActionResult> PMManageUnits()
        {
            try
            {
                // Fetch data from the Units table
                var units = await _context.Units
                    .Where(u => u.UnitStatus == "Active") // Filter for active units
                    .Select(u => new UnitViewModel
                    {
                        UnitId = u.UnitID,
                        UnitName = u.UnitName,
                        PricePerMonth = u.PricePerMonth,
                        UnitStatus = u.UnitStatus
                    })
                    .ToListAsync();

                // Pass the list of units to the view
                return View(units);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading units: {ex.Message}";
                return View(new List<UnitViewModel>()); // Return an empty list in case of error
            }
        }


        [HttpPost]
        public IActionResult UpdateLeaseStatus(LeaseValidationModel model)
        {
            // Check if at least two valid IDs are selected
            if (model.ValidIDs.Count < 2)
            {
                TempData["ShowPopup"] = true; // Indicate that the popup should be shown
                TempData["PopupMessage"] = "Please select at least two valid IDs.";
                TempData["PopupTitle"] = "Insufficient ID!";  // Set the custom title
                TempData["PopupIcon"] = "warning";  // Set the icon dynamically (can be success, error, info, warning)
                ModelState.AddModelError("", "At least two valid IDs must be selected.");
                return RedirectToAction("PMManageLease"); // Return the view with validation errors
            }

            // Check if all selected IDs are valid
            foreach (var id in model.ValidIDs)
            {
                if (!model.IDValidationResults.TryGetValue(id, out bool isValid) || !isValid)
                {
                    TempData["ShowPopup"] = true; // Indicate that the popup should be shown
                    TempData["PopupMessage"] = "Selected IDs must be valid to proceed.";
                    TempData["PopupTitle"] = "Invalid requirement!";  // Set the custom title
                    TempData["PopupIcon"] = "warning";  // Set the icon dynamically (can be success, error, info, warning)
                    ModelState.AddModelError("", "At least two valid IDs must be selected.");
                    ModelState.AddModelError("", $"The ID '{id}' is not valid.");
                    return RedirectToAction("PMManageLease");
                }
            }

            // Check if the lease agreement and security deposit are confirmed
            if (!model.LeaseAgreement || !model.SecurityDeposit)
            {
                TempData["ShowPopup"] = true; // Indicate that the popup should be shown
                TempData["PopupMessage"] = "Applicant must have signed the lease and paid the security deposit.";
                TempData["PopupTitle"] = "Missing requirement!";  // Set the custom title
                TempData["PopupIcon"] = "warning";  // Set the icon dynamically (can be success, error, info, warning)
                ModelState.AddModelError("", "At least two valid IDs must be selected.");
                ModelState.AddModelError("", "Lease agreement and security deposit must be confirmed.");
                return RedirectToAction("PMManageLease");
            }

            // Check if a payment method is selected
            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
            {
                TempData["ShowPopup"] = true; // Indicate that the popup should be shown
                TempData["PopupMessage"] = "Please select the payment method used by the applicant.";
                TempData["PopupTitle"] = "Missing requirement!";  // Set the custom title
                TempData["PopupIcon"] = "warning";  // Set the icon dynamically (can be success, error, info, warning)
                ModelState.AddModelError("", "A payment method must be selected.");
                return RedirectToAction("PMManageLease");
            }

            if (model.LeaseAgreementFile == null)
            {
                TempData["ShowPopup"] = true; // Indicate that the popup should be shown
                TempData["PopupMessage"] = "Please upload the contract.";
                TempData["PopupTitle"] = "Missing requirement!";  // Set the custom title
                TempData["PopupIcon"] = "warning";  // Set the icon dynamically (can be success, error, info, warning)
                return RedirectToAction("PMManageLease");
            }


            // Process the uploaded lease agreement PDF file
            if (model.LeaseAgreementFile != null && model.LeaseAgreementFile.Length > 0)
            {
                // Define the path to save the file in wwwroot/contracts
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contracts");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath); // Create the directory if it does not exist
                }

                var filePath = Path.Combine(folderPath, model.LeaseAgreementFile.FileName);

                // Save the file to the defined path
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.LeaseAgreementFile.CopyTo(stream);
                }

                // Update the LeaseAgreementFilePath in the Lease model
                var currentLease = _context.Leases.FirstOrDefault(l => l.LeaseID == model.LeaseID);
                if (currentLease != null)
                {
                    currentLease.LeaseAgreementFilePath = "/contracts/" + model.LeaseAgreementFile.FileName; // Store relative file path
                    _context.SaveChanges();
                }
            }



            // Retrieve the lease record to get UnitID and TenantID
            var lease = _context.Leases.FirstOrDefault(l => l.LeaseID == model.LeaseID);
            if (lease == null)
            {
                TempData["ShowPopup"] = true;
                TempData["PopupMessage"] = "Lease record not found.";
                TempData["PopupTitle"] = "Error!";
                TempData["PopupIcon"] = "error";
                return RedirectToAction("PMManageLease");
            }

            int? tenantId = lease.TenantID;
            int? unitId = lease.UnitId;

            // Call the stored procedure using ExecuteSqlRaw
            _context.Database.ExecuteSqlRaw("EXEC RMS_SP_UPDATE_A_LEASE @p0, @p1, @p2",
                parameters: new object[] { model.LeaseID, unitId, tenantId });


            TempData["ShowPopup"] = true; // Indicate that the popup should be shown
            TempData["PopupMessage"] = "Lease application has been successfully confirmed";
            TempData["PopupTitle"] = "Tenant Confirmed";  // Set the custom title
            TempData["PopupIcon"] = "success";  // Set the icon dynamically (can be success, error, info, warning)
            return RedirectToAction("PMActiveLease"); // Redirect to a relevant page
        }




        public IActionResult PMPaymentsOrig()
        {
            // Fetch payments and related data from the database
            var payments = _context.Payments
                .Include(p => p.Lease) // Include the Lease navigation property
                .ThenInclude(l => l.LeaseDetails) // Include LeaseDetails for tenant's full name
                .Include(p => p.Lease.Unit) // Include Unit for unit name and monthly rent
                .Select(p => new ManagerPaymentDisplayModel
                {
                    PaymentId = p.PaymentID,
                    TenantFullName = p.Lease.LeaseDetails.FullName, // Tenant full name from LeaseDetails
                    Amount = p.Amount, // Amount from Payments
                    UnitName = p.Lease.Unit.UnitName, // Unit name from Units table
                    MonthlyRent = p.Lease.Unit.PricePerMonth, // Monthly rent from Units table
                    PaymentDate = p.PaymentDate.HasValue ? p.PaymentDate.Value.ToString("MM/dd/yyyy") : "N/A",
                    PaymentMethod = p.PaymentMethod // Payment method from Payments table
                })
                .ToList();

            return View(payments); // Pass the list of PaymentDisplayModel to the view
        }


        public IActionResult ManagerPreviewInvoice(int id)
        {
            // Retrieve the Payment record based on PaymentID (id)
            var payment = _context.Payments
                                  .Include(p => p.Lease)
                                      .ThenInclude(l => l.Unit)
                                  .Include(p => p.Lease)
                                      .ThenInclude(l => l.LeaseDetails)
                                  .FirstOrDefault(p => p.PaymentID == id);

            if (payment == null)
            {
                return NotFound(); // Handle the case where the payment record doesn't exist
            }

            // Get Lease and Tenant Details
            var lease = payment.Lease;
            var tenantName = lease?.LeaseDetails?.FullName;

            // Get Unit Name
            var unitName = lease?.Unit?.UnitName;

            // Prepare the Invoice Preview Model
            var invoicePreviewModel = new InvoicePreviewModel
            {
                PaymentId = payment.PaymentID,
                LeaseNumber = lease?.LeaseID.ToString() ?? "N/A",
                UnitName = unitName ?? "N/A",
                MonthlyRent = payment.Amount ?? 0,
                TenantName = tenantName ?? "N/A",
                PaymentDate = payment.PaymentDate?.Date ?? DateTime.Now.Date,
                PaymentTime = payment.PaymentDate?.ToString("hh:mm tt") ?? "N/A",
                PaymentMethod = payment.PaymentMethod ?? "N/A",
                PaymentStatus = payment.PaymentStatus
            };

            // Pass the model to the view
            return View(invoicePreviewModel);
        }

        public IActionResult PMTenants()
        {
            return View();
        }

        public IActionResult PMManageUsers()
        {
            return View();
        }

        public IActionResult PMEditProfile()
        {
            return View();
        }

        public IActionResult AddUnitPage()
        {
            return View();
        }

        public async Task<IActionResult> EditUnitPage(int id)
        {
            try
            {
                // Fetch the unit details by UnitId
                var unit = await _context.Units.FirstOrDefaultAsync(u => u.UnitID == id && u.UnitStatus == "Active");

                if (unit == null)
                {
                    TempData["ErrorMessage"] = "Unit not found.";
                    return RedirectToAction("PMManageUnits");
                }

                // Fetch images
                var images = _context.UnitImages.Where(i => i.UnitId == id).ToList();
                unit.Images = images; 

                return View(unit);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading unit: {ex.Message}";
                return RedirectToAction("PMManageUnits");
            }
        }


        // Action for adding units
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnit(Unit model, IFormFileCollection Images)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model); // Return the form with validation messages
            //}

            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    // Save Unit details to Units table
                    var newUnit = new Unit
                    {
                        UnitName = model.UnitName,
                        UnitType = model.UnitType,
                        UnitStatus = model.UnitStatus,
                        UnitOwner = model.UnitOwner,
                        Description = model.Description,
                        PricePerMonth = model.PricePerMonth,
                        SecurityDeposit = model.SecurityDeposit,
                        Town = model.Town,
                        Location = model.Location,
                        Country = model.Country,
                        State = model.State,
                        City = model.City,
                        ZipCode = model.ZipCode,
                        NumberOfUnits = model.NumberOfUnits,
                        NumberOfBedrooms = model.NumberOfBedrooms,
                        NumberOfBathrooms = model.NumberOfBathrooms,
                        NumberOfGarages = model.NumberOfGarages,
                        NumberOfFloors = model.NumberOfFloors
                    };

                    _context.Units.Add(newUnit);
                    await _context.SaveChangesAsync();

                    // Save images to UnitImages table and the filesystem
                    if (Images != null && Images.Any())
                    {
                        var imagesFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "units");

                        // Ensure the units folder exists
                        if (!Directory.Exists(imagesFolderPath))
                        {
                            Directory.CreateDirectory(imagesFolderPath);
                        }

                        foreach (var image in Images)
                        {
                            if (image.Length > 0)
                            {
                                // Define the file path to save the image
                                var filePath = Path.Combine(imagesFolderPath, image.FileName);

                                // Save the image to the server
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await image.CopyToAsync(stream);
                                }

                                // Save metadata to the UnitImages table
                                var unitImage = new UnitImage
                                {
                                    UnitId = newUnit.UnitID, // Link to the newly created unit
                                    FilePath = $"/images/units/{image.FileName}" // Relative path for access in views
                                };
                                _context.UnitImages.Add(unitImage);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    TempData["ShowPopup"] = true; // Indicate that the popup shou
                    TempData["PopupMessage"] = "Unit added successfully!";
                }

                return RedirectToAction("PMManageUnits"); // Redirect to the list of units
            }
            catch (Exception ex)
            {
                // Rollback transaction in case of an error
                TempData["ErrorMessage"] = $"Error adding unit: {ex.Message}";
                return RedirectToAction("AddUnitPage");
            }
        }


        // POST: EditUnit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUnit(Unit unit, IFormFileCollection Images)
        {
            if (!ModelState.IsValid)
            {
                // Return to the same page with validation errors
                return View("EditUnitPage", unit);
            }

            // Retrieve the unit from the database using the UnitID
            var existingUnit = await _context.Units.FindAsync(unit.UnitID);
            if (existingUnit == null)
            {
                return NotFound();
            }

            // Update the properties of the unit
            existingUnit.UnitName = unit.UnitName;
            existingUnit.UnitType = unit.UnitType;
            existingUnit.UnitOwner = unit.UnitOwner;
            existingUnit.Description = unit.Description;
            existingUnit.PricePerMonth = unit.PricePerMonth;
            existingUnit.SecurityDeposit = unit.SecurityDeposit;
            existingUnit.Town = unit.Town;
            existingUnit.Location = unit.Location;
            existingUnit.Country = unit.Country;
            existingUnit.State = unit.State;
            existingUnit.City = unit.City;
            existingUnit.ZipCode = unit.ZipCode;
            existingUnit.NumberOfUnits = unit.NumberOfUnits;
            existingUnit.NumberOfBedrooms = unit.NumberOfBedrooms;
            existingUnit.NumberOfBathrooms = unit.NumberOfBathrooms;
            existingUnit.NumberOfGarages = unit.NumberOfGarages;
            existingUnit.NumberOfFloors = unit.NumberOfFloors;
            existingUnit.UnitStatus = unit.UnitStatus;


            // Handle image upload if provided
            if (Images != null && Images.Any())
            {
                // Define the folder path where images will be stored
                var imagesFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "units");

                // Ensure the directory exists
                if (!Directory.Exists(imagesFolderPath))
                {
                    Directory.CreateDirectory(imagesFolderPath);
                }

                // Remove existing images from the database (optional)
                // _context.UnitImages.RemoveRange(existingUnit.Images);
                // await _context.SaveChangesAsync();

                foreach (var image in Images)
                {
                    if (image.Length > 0)
                    {
                        // Define the file path to save the image
                        var filePath = Path.Combine(imagesFolderPath, image.FileName);

                        // Save the image to the server
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Save the image metadata to the UnitImages table
                        var unitImage = new UnitImage
                        {
                            UnitId = existingUnit.UnitID, // Link to the existing unit
                            FilePath = $"/images/units/{image.FileName}" // Relative path for access in views
                        };

                        _context.UnitImages.Add(unitImage);
                    }
                }
            }



            try
            {
                // Save changes to the database
                _context.Update(existingUnit);
                await _context.SaveChangesAsync();
                TempData["ShowPopup"] = true; // Indicate that the popup shou
                TempData["PopupMessage"] = "Unit updated successfully!";
                //TempData.Keep("ShowPopup");
                //TempData.Keep("PopupMessage");

            }
            catch (Exception ex)
            {

                // Log the error and display an error message
                TempData["ErrorMessage"] = $"An error occurred while updating the unit: {ex.Message}";
            }
            // Redirect to the Units page or any other desired page
            return RedirectToAction("PMManageUnits");
        }


        // Soft delete unit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUnit(int id)
        {
            // Retrieve the unit from the database using the UnitID
            var unit = _context.Units.FirstOrDefault(u => u.UnitID == id);
            if (unit == null)
            {
                return NotFound();
            }

            // Set the UnitStatus to "Inactive" for soft deletion
            unit.UnitStatus = "Inactive";

            try
            {
                // Update the unit status and save changes to the database
                _context.Update(unit);
                _context.SaveChanges();  // Synchronous save

                // Set a success message to show in the view
                TempData["ShowPopup"] = true;  // Indicate that the popup should be shown
                TempData["PopupMessage"] = "Unit deleted successfully!";
            }
            catch (Exception ex)
            {
                // Log the error and show an error message
                TempData["ErrorMessage"] = $"An error occurred while deactivating the unit: {ex.Message}";
            }

            // Redirect to the Units page after the action
            return RedirectToAction("PMManageUnits");
        }


        // Action method to search units by name or type
        [HttpGet]
        public IActionResult SearchUnits(string query)
        {
            // Filter out units with "Inactive" UnitStatus
            var activeUnits = _context.Units
                .Where(u => u.UnitStatus == "Active"); // Only include units with UnitStatus as "Active"

            // If query is null or empty, return all units
            if (string.IsNullOrEmpty(query))
            {
                var allActiveUnits = activeUnits.ToList();
                return Json(allActiveUnits); // Return all active units as JSON
            }

            // Filter by UnitName or UnitType (case-insensitive)
            var filteredUnits = activeUnits
                .Where(u => u.UnitName.ToLower().Contains(query.ToLower()) ||
                            u.UnitType.ToLower().Contains(query.ToLower())) // Match query with UnitName or UnitType
                .ToList();

            return Json(filteredUnits); // Return filtered units as JSON
        }

        public IActionResult PMStaff()
        {
            // Retrieve all staff from the database, including related User information
            var staffList = _context.Staffs
                .Where(staff => staff.UserId != null && staff.User.IsActive) // Filter by active users
                .Select(staff => new
                {
                    StaffID = staff.StaffID,
                    StaffName = staff.UserId != null ? staff.User.FirstName + " " + staff.User.LastName : "Unknown",
                    StaffRole = staff.StaffRole,
                    Shift = (staff.ShiftStartTime.HasValue && staff.ShiftEndTime.HasValue)
                        ? (staff.ShiftStartTime.Value.Hour == 6 && staff.ShiftEndTime.Value.Hour == 14 ? "First"
                        : staff.ShiftStartTime.Value.Hour == 14 && staff.ShiftEndTime.Value.Hour == 22 ? "Second"
                        : staff.ShiftStartTime.Value.Hour == 22 || staff.ShiftStartTime.Value.Hour == 0 ? "Third"
                        : "Unknown")
                        : "Unknown",
                    Availability = staff.IsVacant ? "Vacant" : "Occupied"
                })
                .ToList()
                .Select(staff => new StaffViewModel
                {
                    StaffID = staff.StaffID,
                    StaffName = staff.StaffName,
                    StaffRole = staff.StaffRole,
                    Shift = staff.Shift,
                    Availability = staff.Availability
                })
                .ToList();

            return View(staffList);
        }

        [HttpPost]
        public async Task<IActionResult> AddStaff(AddStaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("All fields are required.");
            }

            try
            {
                // Create instance of the Users table
                var newUser = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password, // Consider hashing the password
                    Role = "Staff", // Default role for all staff
                    TermsAndConditions = true // Default value
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Get the generated UserId
                int userId = newUser.UserID;

                // Set default shift times based on the selected shift
                TimeOnly? shiftStartTime = null;
                TimeOnly? shiftEndTime = null;

                switch (model.Shift)
                {
                    case "First":
                        shiftStartTime = TimeOnly.FromTimeSpan(new TimeSpan(6, 0, 0)); // 6 AM
                        shiftEndTime = TimeOnly.FromTimeSpan(new TimeSpan(14, 0, 0)); // 2 PM
                        break;
                    case "Second":
                        shiftStartTime = TimeOnly.FromTimeSpan(new TimeSpan(14, 0, 0)); // 2 PM
                        shiftEndTime = TimeOnly.FromTimeSpan(new TimeSpan(22, 0, 0)); // 10 PM
                        break;
                    case "Third":
                        shiftStartTime = TimeOnly.FromTimeSpan(new TimeSpan(22, 0, 0)); // 10 PM
                        shiftEndTime = TimeOnly.FromTimeSpan(new TimeSpan(6, 0, 0)); // 6 AM
                        break;
                    default:
                        return BadRequest("Invalid shift selected.");
                }

                // Create instance of the Staffs table
                var newStaff = new Staff
                {
                    UserId = userId,
                    StaffRole = model.Role,
                    ShiftStartTime = shiftStartTime,
                    ShiftEndTime = shiftEndTime,
                    IsVacant = true
                };

                _context.Staffs.Add(newStaff);
                await _context.SaveChangesAsync();

                TempData["ShowPopup"] = true; // Indicate that the popup should be shown
                TempData["PopupMessage"] = "Staff added successfully!";
                TempData["PopupTitle"] = "Success!";  // Set the custom title
                TempData["PopupIcon"] = "success";  // Set the icon dynamically (can be success, error, info, warning)

                return RedirectToAction("PMStaff");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //[HttpPost]
        public IActionResult DeleteStaff(int id)
        {
            // Find the staff record using the provided staff ID
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffID == id);
            if (staff == null)
            {
                return NotFound("Staff not found.");
            }

            // Find the associated user using the UserId from the staff record
            var user = _context.Users.FirstOrDefault(u => u.UserID == staff.UserId);
            if (user == null)
            {
                return NotFound("Associated user not found.");
            }

            // Set the IsActive field to false (soft delete)
            user.IsActive = false;

            // Save changes to the database
            _context.SaveChanges();

            TempData["ShowPopup"] = true; // Indicate that the popup should be shown
            TempData["PopupMessage"] = "Staff deleted successfully!";
            TempData["PopupTitle"] = "Success!";  // Set the custom title
            TempData["PopupIcon"] = "success";  // Set the icon dynamically (can be success, error, info, warning)

            return RedirectToAction("PMStaff");
        }

        public IActionResult CancelPendingLease(int id)
        {
            var lease = _context.Leases.FirstOrDefault(l => l.LeaseID == id);
            if (lease == null)
            {
                return NotFound("Lease not found.");
            }

            // Call the stored procedure to cancel the lease
            _context.Database.ExecuteSqlRaw("EXEC RMS_SP_CANCEL_PENDING_LEASE @p0", new object[] { id });

            TempData["ShowPopup"] = true; // Indicate that the popup should be shown
            TempData["PopupMessage"] = "Lease has been cancelled successfully!";
            TempData["PopupTitle"] = "Lease Cancelled!";  // Set the custom title
            TempData["PopupIcon"] = "success";  // Set the icon dynamically (can be success, error, info, warning)
            return RedirectToAction("PMManageLease");
        }

        public IActionResult DeactivateActiveLease(int id)
        {
            var lease = _context.Leases.FirstOrDefault(l => l.LeaseID == id);
            if (lease == null)
            {
                return NotFound("Lease not found.");
            }

            lease.LeaseStatus = "Inactive";
            _context.Leases.Update(lease);

            var tenant = _context.Tenants.FirstOrDefault(t => t.TenantID == lease.TenantID);
            if (tenant == null)
            {
                return NotFound("Tenant not found.");
            }

            tenant.IsActualTenant = false;
            _context.Tenants.Update(tenant);

            var unit = _context.Units.FirstOrDefault(u => u.UnitID == lease.UnitId);
            if (unit == null)
            {
                return NotFound("Unit not found.");
            }

            unit.AvailabilityStatus = "Available";
            _context.Units.Update(unit);

            _context.SaveChanges();


            TempData["ShowPopup"] = true; // Indicate that the popup should be shown
            TempData["PopupMessage"] = "Lease has been deactivated successfully!";
            TempData["PopupTitle"] = "Lease Deactivated!";  // Set the custom title
            TempData["PopupIcon"] = "success";  // Set the icon dynamically (can be success, error, info, warning)
            return RedirectToAction("PMActiveLease");
        }

        public IActionResult PMApplyLease()
        {
            var units = _context.Units
                .Where(u => u.UnitStatus == "Active" && u.AvailabilityStatus == "Available")
                .Select(u => new PMUnitViewModel
                {
                    UnitId = u.UnitID, // Adjust property names to match your database
                    UnitName = u.UnitName
                })
                .ToList();

            return View(units);
        }


        [HttpPost]
        public IActionResult PMApplyForLease(LeaseApplication leaseApplication)
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
                return RedirectToAction("PMApplyLease", new { unitId = leaseApplication.UnitId });
            }

            // Extract FirstName and LastName from FullName
            var fullNameParts = leaseApplication.FullName.Split(' ', 2);
            var firstName = fullNameParts.Length > 0 ? fullNameParts[0] : string.Empty;
            var lastName = fullNameParts.Length > 1 ? fullNameParts[1] : string.Empty;

            // Create a new User instance
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = leaseApplication.Email,
                Password = firstName.ToLower(), // Lowercased first name
                Role = "Tenant",
                TermsAndConditions = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Create a new Tenant instance linked to the User
            var tenant = new Tenant
            {
                UserId = user.UserID
            };

            _context.Tenants.Add(tenant);
            _context.SaveChanges();

            // Calculate LeaseEndDate based on LeaseDuration and LeaseStartDate
            DateTime? leaseEndDate = leaseApplication.LeaseStartDate?.AddMonths(leaseApplication.LeaseDuration.Value);

            // Create a new Lease instance
            var lease = new Lease
            {
                TenantID = tenant.TenantID, // Use the newly created TenantID
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
                TempData["PopupMessage"] = "Lease has been applied successfully for the tenant!";
                TempData["PopupTitle"] = "Success!";  // Set the custom title
                TempData["PopupIcon"] = "success";  // Set the icon dynamically (can be success, error, info, warning)
                return RedirectToAction("PMManageLease"); // Redirect to a success page or confirmation
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the lease application: " + ex.Message);
                return RedirectToAction("PMApplyLease", new { unitId = leaseApplication.UnitId });
            }
        }



        public IActionResult EditLease(EditLeaseFormModel editLeaseFormModel)
        {
            var lease = _context.Leases.FirstOrDefault(l => l.LeaseID == editLeaseFormModel.LeaseId);
            if (lease == null)
            {
                return NotFound("Lease not found.");
            }

            // Check for valid StartDate and EndDate in the form model, and only call the stored procedure if dates are provided
            if (editLeaseFormModel.StartDate != null && editLeaseFormModel.EndDate != null)
            {
                // Call the stored procedure to update the lease's start and end date
                var leaseIdParam = new SqlParameter("@LeaseID", editLeaseFormModel.LeaseId);
                var startDateParam = new SqlParameter("@StartDate", editLeaseFormModel.StartDate);
                var endDateParam = new SqlParameter("@EndDate", editLeaseFormModel.EndDate);

                _context.Database.ExecuteSqlRaw("EXEC RMS_SP_UPDATE_ACTIVE_LEASE @LeaseID, @StartDate, @EndDate", leaseIdParam, startDateParam, endDateParam);
            }

            TempData["ShowPopup"] = true; // Indicate that the popup should be shown
            TempData["PopupMessage"] = "Lease has been updated successfully!";
            TempData["PopupTitle"] = "Success!";  // Set the custom title
            TempData["PopupIcon"] = "success";  // Set the icon dynamically (can be success, error, info, warning)

            return RedirectToAction("PMActiveLease");
        }


        public async Task<IActionResult> PMRequest()
        {
            // Get the logged-in user's ID from the session (for access control purposes if needed)
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if not logged in
            }

            // Fetch all requests where the associated tenant is an actual tenant
            var requests = await _context.Requests
                .Include(r => r.Tenant)
                .ThenInclude(t => t.User) // Include tenant user details
                .Include(r => r.Staff)
                .ThenInclude(s => s.User) // Include staff user details
                .Where(r => r.Tenant.IsActualTenant) // Only include requests for tenants where IsActualTenant is true
                .ToListAsync();

            // Map the requests to a ViewModel
            var requestViewModels = new List<RequestViewModel>();

            foreach (var request in requests)
            {
                // Fetch the unit details via Lease and Unit
                var lease = await _context.Leases
                    .Include(l => l.Unit)
                    .FirstOrDefaultAsync(l => l.TenantID == request.TenantID);

                var unitName = lease?.Unit?.UnitName ?? "N/A";

                // Prepare the ViewModel
                var requestViewModel = new RequestViewModel
                {
                    Tenant = $"{request.Tenant.User?.FirstName} {request.Tenant.User?.LastName}",
                    Unit = unitName,
                    RequestDate = request.RequestDateTime?.ToString("MM/dd/yyyy hh:mm tt") ?? "N/A",
                    Issue = request.RequestType ?? "N/A",
                    AssignedStaff = request.Staff?.User != null
                        ? $"{request.Staff.User.FirstName} {request.Staff.User.LastName}"
                        : "Unassigned",
                    Status = request.RequestStatus ?? "N/A"
                };

                requestViewModels.Add(requestViewModel);
            }

            // Pass the ViewModel list to the view
            return View(requestViewModels);
        }



    }
}
