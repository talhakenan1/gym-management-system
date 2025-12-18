using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Models;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                TotalMembers = await _context.Users.CountAsync(),
                TotalTrainers = await _context.Trainers.CountAsync(),
                TotalServices = await _context.Services.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync(),
                PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending)
            };

            ViewBag.Stats = stats;
            return View();
        }

        // Trainers Management
        public async Task<IActionResult> Trainers()
        {
            var trainers = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .ToListAsync();
            return View(trainers);
        }

        // Services Management
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .Include(s => s.Gym)
                .ToListAsync();
            return View(services);
        }

        // Appointments Management
        public async Task<IActionResult> Appointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        public async Task<IActionResult> RejectAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Appointments));
        }

        // Download Report
        public async Task<IActionResult> DownloadReport()
        {
            var reportDate = DateTime.Now;
            
            // Gather statistics
            var totalMembers = await _context.Users.CountAsync();
            var totalTrainers = await _context.Trainers.CountAsync();
            var totalServices = await _context.Services.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();
            var pendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending);
            var confirmedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Confirmed);
            var completedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);
            var cancelledAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled);

            // Get recent appointments
            var recentAppointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.CreatedAt)
                .Take(20)
                .ToListAsync();

            // Get trainers list
            var trainers = await _context.Trainers
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .ToListAsync();

            // Get services list
            var services = await _context.Services.ToListAsync();

            // Build CSV content
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("FitLife Gym - Yönetim Raporu");
            csv.AppendLine($"Rapor Tarihi: {reportDate:dd.MM.yyyy HH:mm}");
            csv.AppendLine();
            csv.AppendLine("=== GENEL İSTATİSTİKLER ===");
            csv.AppendLine($"Toplam Üye Sayısı: {totalMembers}");
            csv.AppendLine($"Toplam Eğitmen Sayısı: {totalTrainers}");
            csv.AppendLine($"Toplam Hizmet Sayısı: {totalServices}");
            csv.AppendLine($"Toplam Randevu Sayısı: {totalAppointments}");
            csv.AppendLine();
            csv.AppendLine("=== RANDEVU DURUMU DAĞILIMI ===");
            csv.AppendLine($"Bekleyen Randevular: {pendingAppointments}");
            csv.AppendLine($"Onaylanan Randevular: {confirmedAppointments}");
            csv.AppendLine($"Tamamlanan Randevular: {completedAppointments}");
            csv.AppendLine($"İptal Edilen Randevular: {cancelledAppointments}");
            csv.AppendLine();
            csv.AppendLine("=== EĞİTMEN LİSTESİ ===");
            csv.AppendLine("Ad Soyad;Uzmanlık;Email;Telefon;Aktif");
            foreach (var trainer in trainers)
            {
                csv.AppendLine($"{trainer.FirstName} {trainer.LastName};{trainer.Specialization ?? "-"};{trainer.Email ?? "-"};{trainer.Phone ?? "-"};{(trainer.IsActive ? "Evet" : "Hayır")}");
            }
            csv.AppendLine();
            csv.AppendLine("=== HİZMET LİSTESİ ===");
            csv.AppendLine("Hizmet Adı;Süre (dk);Fiyat;Aktif");
            foreach (var service in services)
            {
                csv.AppendLine($"{service.Name};{service.Duration};{service.Price:C};{(service.IsActive ? "Evet" : "Hayır")}");
            }
            csv.AppendLine();
            csv.AppendLine("=== SON 20 RANDEVU ===");
            csv.AppendLine("Randevu No;Müşteri;Eğitmen;Hizmet;Tarih;Saat;Durum;Fiyat");
            foreach (var apt in recentAppointments)
            {
                var statusText = apt.Status switch
                {
                    AppointmentStatus.Pending => "Beklemede",
                    AppointmentStatus.Confirmed => "Onaylandı",
                    AppointmentStatus.Completed => "Tamamlandı",
                    AppointmentStatus.Cancelled => "İptal",
                    _ => "Bilinmiyor"
                };
                csv.AppendLine($"{apt.Id};{apt.User?.FirstName} {apt.User?.LastName};{apt.Trainer?.FirstName} {apt.Trainer?.LastName};{apt.Service?.Name ?? "-"};{apt.AppointmentDate:dd.MM.yyyy};{apt.StartTime:hh\\:mm}-{apt.EndTime:hh\\:mm};{statusText};{apt.TotalPrice:C}");
            }

            var fileName = $"FitLife_Rapor_{reportDate:yyyyMMdd_HHmmss}.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            
            // Add BOM for Excel UTF-8 compatibility
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var result = new byte[bom.Length + bytes.Length];
            bom.CopyTo(result, 0);
            bytes.CopyTo(result, bom.Length);

            return File(result, "text/csv; charset=utf-8", fileName);
        }
    }
}