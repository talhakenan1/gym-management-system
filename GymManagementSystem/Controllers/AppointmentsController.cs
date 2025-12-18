using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Models;

namespace GymManagementSystem.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            var trainers = await _context.Trainers.Where(t => t.IsActive).ToListAsync();
            var services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            
            ViewData["TrainerId"] = new SelectList(trainers, "Id", "FirstName");
            ViewData["ServiceId"] = new SelectList(services, "Id", "Name");
            ViewBag.Services = services; // JavaScript için service bilgileri
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrainerId,ServiceId,AppointmentDate,StartTime,Notes")] Appointment appointment)
        {
            // Remove navigation property validation errors
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("Trainer");
            ModelState.Remove("Service");
            
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "Kullanıcı bilgisi bulunamadı.");
                    var trainers2 = await _context.Trainers.Where(t => t.IsActive).ToListAsync();
                    var services2 = await _context.Services.Where(s => s.IsActive).ToListAsync();
                    ViewData["TrainerId"] = new SelectList(trainers2, "Id", "FirstName");
                    ViewData["ServiceId"] = new SelectList(services2, "Id", "Name");
                    ViewBag.Services = services2;
                    return View(appointment);
                }

                appointment.UserId = userId;

                // Get service details
                var service = await _context.Services.FindAsync(appointment.ServiceId);
                if (service == null)
                {
                    ModelState.AddModelError("ServiceId", "Seçilen hizmet bulunamadı.");
                }
                else
                {
                    appointment.EndTime = appointment.StartTime.Add(TimeSpan.FromMinutes(service.Duration));
                    appointment.TotalPrice = service.Price;
                }

                // Check trainer availability for this day
                var dayOfWeek = appointment.AppointmentDate.DayOfWeek;
                var trainerAvailability = await _context.TrainerAvailabilities
                    .FirstOrDefaultAsync(ta => ta.TrainerId == appointment.TrainerId &&
                                              ta.DayOfWeek == dayOfWeek &&
                                              ta.IsActive);

                if (trainerAvailability == null)
                {
                    ModelState.AddModelError("", "Antrenör bu gün çalışmıyor.");
                }
                else if (appointment.StartTime < trainerAvailability.StartTime || 
                         appointment.EndTime > trainerAvailability.EndTime)
                {
                    ModelState.AddModelError("", "Seçilen saat antrenörün çalışma saatleri dışında.");
                }

                // Check for conflicting appointments
                var hasConflict = await _context.Appointments
                    .AnyAsync(a => a.TrainerId == appointment.TrainerId &&
                                  a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                                  a.Status != AppointmentStatus.Cancelled &&
                                  ((appointment.StartTime >= a.StartTime && appointment.StartTime < a.EndTime) ||
                                   (appointment.EndTime > a.StartTime && appointment.EndTime <= a.EndTime) ||
                                   (appointment.StartTime <= a.StartTime && appointment.EndTime >= a.EndTime)));

                if (hasConflict)
                {
                    ModelState.AddModelError("", "Bu saatte antrenör müsait değil. Lütfen başka bir saat seçin.");
                }

                if (ModelState.IsValid)
                {
                    appointment.CreatedAt = DateTime.Now;
                    appointment.Status = AppointmentStatus.Pending;
                    
                    _context.Add(appointment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Randevunuz başarıyla oluşturuldu. Onay bekliyor.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Randevu oluşturulurken bir hata oluştu: " + ex.Message);
            }

            // Reload data for form
            var trainers = await _context.Trainers.Where(t => t.IsActive).ToListAsync();
            var services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            
            ViewData["TrainerId"] = new SelectList(trainers, "Id", "FirstName", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(services, "Id", "Name", appointment.ServiceId);
            ViewBag.Services = services;
            return View(appointment);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            // Check if user owns this appointment or is admin
            var userId = _userManager.GetUserId(User);
            if (appointment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(appointment);
        }

        // POST: Appointments/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (appointment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Randevunuz iptal edildi.";
            return RedirectToAction(nameof(Index));
        }

        // API endpoint for getting available trainers for a service
        [HttpGet]
        public async Task<JsonResult> GetAvailableTrainers(int serviceId)
        {
            var trainers = await _context.TrainerServices
                .Where(ts => ts.ServiceId == serviceId && ts.Trainer.IsActive)
                .Select(ts => new { 
                    value = ts.TrainerId, 
                    text = ts.Trainer.FirstName + " " + ts.Trainer.LastName 
                })
                .ToListAsync();

            return Json(trainers);
        }

        // API endpoint for checking trainer availability
        [HttpGet]
        public async Task<JsonResult> CheckAvailability(int trainerId, DateTime date, TimeSpan startTime, int duration)
        {
            var endTime = startTime.Add(TimeSpan.FromMinutes(duration));
            
            var hasConflict = await _context.Appointments
                .AnyAsync(a => a.TrainerId == trainerId &&
                              a.AppointmentDate.Date ==  date.Date &&
                              a.Status != AppointmentStatus.Cancelled &&
                              ((startTime >= a.StartTime && startTime < a.EndTime) ||
                               (endTime > a.StartTime && endTime <= a.EndTime) ||
                               (startTime <= a.StartTime && endTime >= a.EndTime)));

            return Json(new { available = !hasConflict });
        }

        // API endpoint for getting available time slots for a trainer on a specific date
        [HttpGet]
        public async Task<JsonResult> GetAvailableTimeSlots(int trainerId, DateTime date, int duration)
        {
            var dayOfWeek = date.DayOfWeek;
            
            // Get trainer's availability for this day
            var availability = await _context.TrainerAvailabilities
                .Where(ta => ta.TrainerId == trainerId && 
                            ta.DayOfWeek == dayOfWeek && 
                            ta.IsActive)
                .FirstOrDefaultAsync();

            if (availability == null)
            {
                return Json(new List<object>());
            }

            // Get existing appointments for this trainer on this date
            var existingAppointments = await _context.Appointments
                .Where(a => a.TrainerId == trainerId &&
                           a.AppointmentDate.Date == date.Date &&
                           a.Status != AppointmentStatus.Cancelled)
                .Select(a => new { a.StartTime, a.EndTime })
                .ToListAsync();

            // Generate available time slots
            var timeSlots = new List<object>();
            var currentTime = availability.StartTime;
            var slotDuration = TimeSpan.FromMinutes(duration);

            while (currentTime.Add(slotDuration) <= availability.EndTime)
            {
                var slotEndTime = currentTime.Add(slotDuration);
                
                // Check if this slot conflicts with any existing appointment
                var hasConflict = existingAppointments.Any(apt =>
                    (currentTime >= apt.StartTime && currentTime < apt.EndTime) ||
                    (slotEndTime > apt.StartTime && slotEndTime <= apt.EndTime) ||
                    (currentTime <= apt.StartTime && slotEndTime >= apt.EndTime));

                if (!hasConflict)
                {
                    timeSlots.Add(new
                    {
                        value = currentTime.ToString(@"hh\:mm\:ss"),
                        text = currentTime.ToString(@"hh\:mm")
                    });
                }

                currentTime = currentTime.Add(TimeSpan.FromMinutes(30)); // 30-minute intervals
            }

            return Json(timeSlots);
        }
    }
}