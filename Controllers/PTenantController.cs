using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Models;

namespace PMS.Controllers
{
    public class PTenantController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PTenantController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult PTenantHomePage()
        {
            return View();
        }

        public async Task<IActionResult> PTenantUnits()
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
