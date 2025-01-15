using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Models;

namespace PMS.Controllers
{
    public class PropertyManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PropertyManagerController(ApplicationDbContext context)
        {
            _context = context;
        }


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

        public IActionResult AddUnitPage()
        {
            return View();
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
                    TempData["SuccessMessage"] = "Unit added successfully!";
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

    }
}
