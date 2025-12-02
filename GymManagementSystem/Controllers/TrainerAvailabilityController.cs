using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Models;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainerAvailabilityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerAvailabilityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TrainerAvailability
        public async Task<IActionResult> Index(int? trainerId)
        {
            var trainers = await _context.Trainers.Where(t => t.IsActive).ToListAsync();
            ViewBag.Trainers = new SelectList(trainers, "Id", "FirstName");
            ViewBag.SelectedTrainerId = trainerId;

            var availabilities = _context.TrainerAvailabilities
                .Include(ta => ta.Trainer)
                .AsQueryable();

            if (trainerId.HasValue)
            {
                availabilities = availabilities.Where(ta => ta.TrainerId == trainerId);
            }

            var result = await availabilities
                .OrderBy(ta => ta.TrainerId)
                .ThenBy(ta => ta.DayOfWeek)
                .ThenBy(ta => ta.StartTime)
                .ToListAsync();

            return View(result);
        }

        // GET: TrainerAvailability/Create
        public IActionResult Create(int? trainerId)
        {
            ViewData["TrainerId"] = new SelectList(_context.Trainers.Where(t => t.IsActive), "Id", "FirstName", trainerId);
            
            var model = new TrainerAvailability();
            if (trainerId.HasValue)
            {
                model.TrainerId = trainerId.Value;
            }

            return View(model);
        }

        // POST: TrainerAvailability/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrainerId,DayOfWeek,StartTime,EndTime,IsAvailable")] TrainerAvailability trainerAvailability)
        {
            // Remove Trainer validation error
            ModelState.Remove("Trainer");
            
            // Check for overlapping availability
            var hasOverlap = await _context.TrainerAvailabilities
                .AnyAsync(ta => ta.TrainerId == trainerAvailability.TrainerId &&
                               ta.DayOfWeek == trainerAvailability.DayOfWeek &&
                               ta.IsActive &&
                               ((trainerAvailability.StartTime >= ta.StartTime && trainerAvailability.StartTime < ta.EndTime) ||
                                (trainerAvailability.EndTime > ta.StartTime && trainerAvailability.EndTime <= ta.EndTime) ||
                                (trainerAvailability.StartTime <= ta.StartTime && trainerAvailability.EndTime >= ta.EndTime)));

            if (hasOverlap)
            {
                ModelState.AddModelError("", "Bu zaman dilimi için çakışan bir müsaitlik kaydı bulunmaktadır.");
            }

            if (trainerAvailability.StartTime >= trainerAvailability.EndTime)
            {
                ModelState.AddModelError("EndTime", "Bitiş saati başlangıç saatinden sonra olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                trainerAvailability.IsActive = true;
                if (!trainerAvailability.IsAvailable)
                {
                    trainerAvailability.IsAvailable = true; // Default to available
                }
                _context.Add(trainerAvailability);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Müsaitlik kaydı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index), new { trainerId = trainerAvailability.TrainerId });
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Doğrulama hataları: " + string.Join(", ", errors);
            }

            ViewData["TrainerId"] = new SelectList(_context.Trainers.Where(t => t.IsActive), "Id", "FirstName", trainerAvailability.TrainerId);
            return View(trainerAvailability);
        }

        // GET: TrainerAvailability/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trainerAvailability = await _context.TrainerAvailabilities.FindAsync(id);
            if (trainerAvailability == null) return NotFound();

            ViewData["TrainerId"] = new SelectList(_context.Trainers.Where(t => t.IsActive), "Id", "FirstName", trainerAvailability.TrainerId);
            return View(trainerAvailability);
        }

        // POST: TrainerAvailability/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrainerId,DayOfWeek,StartTime,EndTime,IsAvailable,IsActive")] TrainerAvailability trainerAvailability)
        {
            if (id != trainerAvailability.Id) return NotFound();

            // Remove Trainer validation error
            ModelState.Remove("Trainer");
            
            // Check for overlapping availability (excluding current record)
            var hasOverlap = await _context.TrainerAvailabilities
                .AnyAsync(ta => ta.Id != id &&
                               ta.TrainerId == trainerAvailability.TrainerId &&
                               ta.DayOfWeek == trainerAvailability.DayOfWeek &&
                               ta.IsActive &&
                               ((trainerAvailability.StartTime >= ta.StartTime && trainerAvailability.StartTime < ta.EndTime) ||
                                (trainerAvailability.EndTime > ta.StartTime && trainerAvailability.EndTime <= ta.EndTime) ||
                                (trainerAvailability.StartTime <= ta.StartTime && trainerAvailability.EndTime >= ta.EndTime)));

            if (hasOverlap)
            {
                ModelState.AddModelError("", "Bu zaman dilimi için çakışan bir müsaitlik kaydı bulunmaktadır.");
            }

            if (trainerAvailability.StartTime >= trainerAvailability.EndTime)
            {
                ModelState.AddModelError("EndTime", "Bitiş saati başlangıç saatinden sonra olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original entity from database
                    var originalEntity = await _context.TrainerAvailabilities.FindAsync(id);
                    if (originalEntity == null) return NotFound();

                    // Update the properties
                    originalEntity.TrainerId = trainerAvailability.TrainerId;
                    originalEntity.DayOfWeek = trainerAvailability.DayOfWeek;
                    originalEntity.StartTime = trainerAvailability.StartTime;
                    originalEntity.EndTime = trainerAvailability.EndTime;
                    originalEntity.IsAvailable = trainerAvailability.IsAvailable;
                    originalEntity.IsActive = trainerAvailability.IsActive;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Müsaitlik kaydı başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerAvailabilityExists(trainerAvailability.Id))
                        return NotFound();
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu: " + ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : ""));
                    ViewData["TrainerId"] = new SelectList(_context.Trainers.Where(t => t.IsActive), "Id", "FirstName", trainerAvailability.TrainerId);
                    return View(trainerAvailability);
                }
                return RedirectToAction(nameof(Index), new { trainerId = trainerAvailability.TrainerId });
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Doğrulama hataları: " + string.Join(", ", errors);
            }

            ViewData["TrainerId"] = new SelectList(_context.Trainers.Where(t => t.IsActive), "Id", "FirstName", trainerAvailability.TrainerId);
            return View(trainerAvailability);
        }

        // GET: TrainerAvailability/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trainerAvailability = await _context.TrainerAvailabilities
                .Include(ta => ta.Trainer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainerAvailability == null) return NotFound();

            return View(trainerAvailability);
        }

        // POST: TrainerAvailability/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainerAvailability = await _context.TrainerAvailabilities.FindAsync(id);
            if (trainerAvailability != null)
            {
                _context.TrainerAvailabilities.Remove(trainerAvailability);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Müsaitlik kaydı başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TrainerAvailabilityExists(int id)
        {
            return _context.TrainerAvailabilities.Any(e => e.Id == id);
        }

        // API endpoint for getting trainer availability for a specific date
        [HttpGet]
        public async Task<JsonResult> GetTrainerAvailability(int trainerId, DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            
            var availability = await _context.TrainerAvailabilities
                .Where(ta => ta.TrainerId == trainerId && 
                            ta.DayOfWeek == dayOfWeek && 
                            ta.IsActive)
                .Select(ta => new { 
                    startTime = ta.StartTime.ToString(@"hh\:mm"), 
                    endTime = ta.EndTime.ToString(@"hh\:mm") 
                })
                .ToListAsync();

            return Json(availability);
        }
    }
}