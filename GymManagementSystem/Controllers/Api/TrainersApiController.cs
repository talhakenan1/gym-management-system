using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Models;

namespace GymManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/trainers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            var trainers = await _context.Trainers
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.Gym)
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Email,
                    t.Phone,
                    t.Specialization,
                    t.Experience,
                    t.IsActive,
                    Gym = new { t.Gym.Name, t.Gym.Id },
                    Services = t.TrainerServices.Select(ts => new
                    {
                        ts.Service.Id,
                        ts.Service.Name,
                        ts.Service.Price,
                        ts.Service.Duration
                    })
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // GET: api/trainers/available/2024-01-15
        [HttpGet("available/{date}")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableTrainers(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            
            var availableTrainers = await _context.Trainers
                .Include(t => t.TrainerAvailabilities)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.Appointments)
                .Where(t => t.IsActive && 
                           t.TrainerAvailabilities.Any(ta => ta.DayOfWeek == dayOfWeek && ta.IsAvailable))
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Specialization,
                    t.Experience,
                    AvailableSlots = t.TrainerAvailabilities
                        .Where(ta => ta.DayOfWeek == dayOfWeek && ta.IsAvailable)
                        .Select(ta => new
                        {
                            ta.StartTime,
                            ta.EndTime
                        }),
                    BookedSlots = t.Appointments
                        .Where(a => a.AppointmentDate.Date == date.Date && 
                                   a.Status != AppointmentStatus.Cancelled)
                        .Select(a => new
                        {
                            a.StartTime,
                            a.EndTime
                        }),
                    Services = t.TrainerServices.Select(ts => new
                    {
                        ts.Service.Id,
                        ts.Service.Name,
                        ts.Service.Price,
                        ts.Service.Duration
                    })
                })
                .ToListAsync();

            return Ok(availableTrainers);
        }

        // GET: api/trainers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTrainer(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.TrainerAvailabilities)
                .Include(t => t.Gym)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    t.Email,
                    t.Phone,
                    t.Specialization,
                    t.Experience,
                    t.Bio,
                    t.IsActive,
                    Gym = new { t.Gym.Name, t.Gym.Id },
                    Services = t.TrainerServices.Select(ts => new
                    {
                        ts.Service.Id,
                        ts.Service.Name,
                        ts.Service.Description,
                        ts.Service.Price,
                        ts.Service.Duration
                    }),
                    Availability = t.TrainerAvailabilities.Select(ta => new
                    {
                        ta.DayOfWeek,
                        ta.StartTime,
                        ta.EndTime,
                        ta.IsAvailable
                    })
                })
                .FirstOrDefaultAsync();

            if (trainer == null)
            {
                return NotFound();
            }

            return Ok(trainer);
        }
    }
}