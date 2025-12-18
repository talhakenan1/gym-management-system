using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Models;
using System.Security.Claims;

namespace GymManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/appointments/user/userId
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserAppointments(string userId)
        {
            // Check if user is requesting their own appointments or is admin
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != userId && !isAdmin)
            {
                return Forbid();
            }

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.StartTime,
                    a.EndTime,
                    a.TotalPrice,
                    a.Status,
                    a.Notes,
                    a.CreatedAt,
                    Trainer = new
                    {
                        a.Trainer.Id,
                        a.Trainer.FirstName,
                        a.Trainer.LastName,
                        a.Trainer.Specialization
                    },
                    Service = new
                    {
                        a.Service.Id,
                        a.Service.Name,
                        a.Service.Duration,
                        a.Service.Price
                    }
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/appointments/trainer/5/date/2024-01-15
        [HttpGet("trainer/{trainerId}/date/{date}")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainerAppointments(int trainerId, DateTime date)
        {
            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .Where(a => a.TrainerId == trainerId && 
                           a.AppointmentDate.Date == date.Date &&
                           a.Status != AppointmentStatus.Cancelled)
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.StartTime,
                    a.EndTime,
                    a.Status,
                    a.TotalPrice,
                    User = new
                    {
                        a.User.FirstName,
                        a.User.LastName,
                        a.User.Email
                    },
                    Service = new
                    {
                        a.Service.Name,
                        a.Service.Duration
                    }
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/appointments/pending
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingAppointments()
        {
            var pendingAppointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.Status == AppointmentStatus.Pending)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.StartTime,
                    a.EndTime,
                    a.TotalPrice,
                    a.CreatedAt,
                    User = new
                    {
                        a.User.FirstName,
                        a.User.LastName,
                        a.User.Email
                    },
                    Trainer = new
                    {
                        a.Trainer.FirstName,
                        a.Trainer.LastName,
                        a.Trainer.Specialization
                    },
                    Service = new
                    {
                        a.Service.Name,
                        a.Service.Duration
                    }
                })
                .ToListAsync();

            return Ok(pendingAppointments);
        }

        // GET: api/appointments/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetAppointmentStatistics()
        {
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var stats = new
            {
                TotalAppointments = await _context.Appointments.CountAsync(),
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today),
                ThisWeekAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate >= thisWeek && a.AppointmentDate < thisWeek.AddDays(7)),
                ThisMonthAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate >= thisMonth && a.AppointmentDate < thisMonth.AddMonths(1)),
                PendingAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Pending),
                ConfirmedAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Confirmed),
                CompletedAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Completed),
                CancelledAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Cancelled),
                TotalRevenue = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .SumAsync(a => a.TotalPrice),
                ThisMonthRevenue = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed && 
                               a.AppointmentDate >= thisMonth && 
                               a.AppointmentDate < thisMonth.AddMonths(1))
                    .SumAsync(a => a.TotalPrice)
            };

            return Ok(stats);
        }
    }
}