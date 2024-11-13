using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ecos.Areas.Identity.Data;
using ecos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ecos.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;



namespace ecos
{
    [Authorize]
    public class ElectricityRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ElectricityRecordsController> _logger;
        private readonly CohereService _cohereService;

        public ElectricityRecordsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ElectricityRecordsController> logger, CohereService cohereService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _cohereService = cohereService;
        }


        // Method to generate AI insights based on electricity records
        private async Task<string> GenerateAIInsights(List<ElectricityRecord> records)
        {
            var prompt = "Given the following electricity records, provide insights on energy usage consumption like increase or decrease percentage. Records: ";
            foreach (var record in records)
            {
                prompt += $"\nMonth: {record.Month.ToString("MMMM yyyy")}, Rate: {record.ElectricityRate}, Bill: {record.TotalBill}";
            }

            return await _cohereService.GetElectricityInsights(prompt);
        }

        // GET: ElectricityRecords
        // GET: ElectricityRecords
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); // Get current logged-in user's ID
            var sortedRecords = await _context.ElectricityRecords
                                              .Where(r => r.UserId == userId) // Filter records by the current user's ID
                                              .OrderByDescending(r => r.Month)
                                              .ToListAsync();

            if (sortedRecords.Count >= 3) // Only call OpenAI if at least 3 records exist
            {
                var insights = await GenerateAIInsights(sortedRecords);
                ViewBag.Insights = insights;
            }
            else
            {
                ViewBag.Insights = "Please add at least 3 records to generate insights.";
            }

            return View(sortedRecords);
        }


        // GET: ElectricityRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var electricityRecord = await _context.ElectricityRecords
                .FirstOrDefaultAsync(m => m.Id == id);
            if (electricityRecord == null)
            {
                return NotFound();
            }

            return View(electricityRecord);
        }

        // GET: ElectricityRecords/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User); // Get the logged-in user

            // Create a list with only the logged-in user's data for the dropdown
            ViewBag.Users = new SelectList(new[] { new { Id = user.Id, Email = user.Email } }, "Id", "Email");

            return View();
        }

        // POST: ElectricityRecords/Create
        // POST: ElectricityRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ElectricityRate,TotalBill,Month")] ElectricityRecord electricityRecord)
        {
            var user = await _userManager.GetUserAsync(User); // Get the logged-in user
            var userId = user.Id;
            var householdName = user.Type; // Example: using Email as HouseholdName. Adjust as needed.

            electricityRecord.UserId = userId;
            electricityRecord.HouseholdName = householdName;
            electricityRecord.DateCreated = DateTime.Now;
            _context.Add(electricityRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }



        // GET: ElectricityRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var electricityRecord = await _context.ElectricityRecords.FindAsync(id);
            if (electricityRecord == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User); // Get the logged-in user's ID

            // Ensure the logged-in user can only edit their own data
            if (electricityRecord.UserId != userId)
            {
                return Unauthorized(); // Prevent access if the record doesn't belong to the user
            }

            // Logging for debugging
            _logger.LogInformation($"Editing record with ID: {electricityRecord.Id} for User ID: {userId}");

            var user = await _userManager.GetUserAsync(User); // Get the logged-in user
            ViewBag.Users = new SelectList(new[] { new { Id = user.Id, Email = user.Email } }, "Id", "Email");

            return View(electricityRecord);
        }

        // POST: ElectricityRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HouseholdName,ElectricityRate,TotalBill,Month,UserId")] ElectricityRecord electricityRecord)
        {
            if (id != electricityRecord.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User); // Get the logged-in user's ID
            var recordToUpdate = await _context.ElectricityRecords.FindAsync(id);

            // Ensure the logged-in user can only edit their own data
            if (recordToUpdate == null || recordToUpdate.UserId != userId)
            {
                return Unauthorized(); // Prevent modification if the record doesn't belong to the user
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the record properties
                    recordToUpdate.HouseholdName = electricityRecord.HouseholdName;
                    recordToUpdate.ElectricityRate = electricityRecord.ElectricityRate;
                    recordToUpdate.TotalBill = electricityRecord.TotalBill;
                    recordToUpdate.Month = electricityRecord.Month;

                    _context.Update(recordToUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ElectricityRecordExists(electricityRecord.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Rethrow the exception if it's not a concurrency issue
                    }
                }
            }

            // Log model state errors if invalid
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                _logger.LogError(error.ErrorMessage);
            }

            return View(electricityRecord);
        }

        // GET: ElectricityRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var electricityRecord = await _context.ElectricityRecords
                .FirstOrDefaultAsync(m => m.Id == id);
            if (electricityRecord == null)
            {
                return NotFound();
            }

            return View(electricityRecord);
        }

        // POST: ElectricityRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var electricityRecord = await _context.ElectricityRecords.FindAsync(id);
            if (electricityRecord != null)
            {
                _context.ElectricityRecords.Remove(electricityRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ElectricityRecordExists(int id)
        {
            return _context.ElectricityRecords.Any(e => e.Id == id);
        }
    }
}
