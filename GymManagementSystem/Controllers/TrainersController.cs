using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Models;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trainers
        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .ToListAsync();
            return View(trainers);
        }

        // GET: Trainers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.TrainerAvailabilities)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        // GET: Trainers/Create
        public IActionResult Create()
        {
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name");
            ViewData["Services"] = new MultiSelectList(_context.Services, "Id", "Name");
            return View();
        }

        // POST: Trainers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Phone,Specialization,Bio,Experience,GymId")] Trainer trainer, int[] selectedServices)
        {
            // Remove Gym validation error since we're setting it manually
            ModelState.Remove("Gym");
            
            if (ModelState.IsValid)
            {
                try
                {
                    trainer.IsActive = true;
                    _context.Add(trainer);
                    await _context.SaveChangesAsync();

                    // Add selected services
                    if (selectedServices != null && selectedServices.Length > 0)
                    {
                        foreach (var serviceId in selectedServices)
                        {
                            _context.TrainerServices.Add(new TrainerService
                            {
                                TrainerId = trainer.Id,
                                ServiceId = serviceId
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = "Antrenör başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Antrenör eklenirken bir hata oluştu: " + ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : ""));
                }
            }
            else
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Doğrulama hataları: " + string.Join(", ", errors);
            }
            
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            ViewData["Services"] = new MultiSelectList(_context.Services, "Id", "Name", selectedServices);
            return View(trainer);
        }

        // GET: Trainers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null) return NotFound();

            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            ViewData["Services"] = new MultiSelectList(_context.Services, "Id", "Name", 
                trainer.TrainerServices.Select(ts => ts.ServiceId));
            return View(trainer);
        }

        // POST: Trainers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Phone,Specialization,Bio,Experience,IsActive,GymId")] Trainer trainer, int[] selectedServices)
        {
            if (id != trainer.Id) return NotFound();

            // Remove Gym validation error
            ModelState.Remove("Gym");
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);

                    // Update trainer services
                    var existingServices = _context.TrainerServices.Where(ts => ts.TrainerId == id);
                    _context.TrainerServices.RemoveRange(existingServices);

                    if (selectedServices != null && selectedServices.Length > 0)
                    {
                        foreach (var serviceId in selectedServices)
                        {
                            _context.TrainerServices.Add(new TrainerService
                            {
                                TrainerId = trainer.Id,
                                ServiceId = serviceId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Antrenör başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
                        return NotFound();
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu: " + ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : ""));
                    ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
                    ViewData["Services"] = new MultiSelectList(_context.Services, "Id", "Name", selectedServices);
                    return View(trainer);
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Doğrulama hataları: " + string.Join(", ", errors);
            }
            
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            ViewData["Services"] = new MultiSelectList(_context.Services, "Id", "Name", selectedServices);
            return View(trainer);
        }

        // GET: Trainers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        // POST: Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }
    }
}