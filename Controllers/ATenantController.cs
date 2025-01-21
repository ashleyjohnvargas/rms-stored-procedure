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

            return View(unitViewModels); // Return the view with the unit list
        }


        [HttpGet]
        public IActionResult ATenantDetails(int id)
        {
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

            return View(model); // Render the PTenantDetails view with the model
        }

        public IActionResult ATenantApply()
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
