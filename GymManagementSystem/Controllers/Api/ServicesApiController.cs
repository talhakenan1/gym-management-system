using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Models;

namespace GymManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/services
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetServices()
        {
            var services = await _context.Services
                .Include(s => s.Gym)
                .Include(s => s.TrainerServices)
                    .ThenInclude(ts => ts.Trainer)
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Price,
                    s.Duration,
                    s.Category,
                    s.IsActive,
                    Gym = new { s.Gym.Name, s.Gym.Id },
                    AvailableTrainers = s.TrainerServices
                        .Where(ts => ts.Trainer.IsActive)
                        .Select(ts => new
                        {
                            ts.Trainer.Id,
                            ts.Trainer.FirstName,
                            ts.Trainer.LastName,
                            ts.Trainer.Specialization,
                            ts.Trainer.Experience
                        })
                })
                .ToListAsync();

            return Ok(services);
        }

        // GET: api/services/category/fitness
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<object>>> GetServicesByCategory(string category)
        {
            var services = await _context.Services
                .Include(s => s.Gym)
                .Include(s => s.TrainerServices)
                    .ThenInclude(ts => ts.Trainer)
                .Where(s => s.IsActive && s.Category.ToLower() == category.ToLower())
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Price,
                    s.Duration,
                    s.Category,
                    Gym = new { s.Gym.Name, s.Gym.Id },
                    AvailableTrainers = s.TrainerServices
                        .Where(ts => ts.Trainer.IsActive)
                        .Select(ts => new
                        {
                            ts.Trainer.Id,
                            ts.Trainer.FirstName,
                            ts.Trainer.LastName,
                            ts.Trainer.Specialization
                        })
                })
                .ToListAsync();

            return Ok(services);
        }

        // GET: api/services/price-range?min=50&max=200
        [HttpGet("price-range")]
        public async Task<ActionResult<IEnumerable<object>>> GetServicesByPriceRange([FromQuery] decimal? min, [FromQuery] decimal? max)
        {
            var query = _context.Services
                .Include(s => s.Gym)
                .Include(s => s.TrainerServices)
                    .ThenInclude(ts => ts.Trainer)
                .Where(s => s.IsActive);

            if (min.HasValue)
            {
                query = query.Where(s => s.Price >= min.Value);
            }

            if (max.HasValue)
            {
                query = query.Where(s => s.Price <= max.Value);
            }

            var services = await query
                .OrderBy(s => s.Price)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Price,
                    s.Duration,
                    s.Category,
                    Gym = new { s.Gym.Name, s.Gym.Id },
                    TrainerCount = s.TrainerServices.Count(ts => ts.Trainer.IsActive)
                })
                .ToListAsync();

            return Ok(services);
        }

        // GET: api/services/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetService(int id)
        {
            var service = await _context.Services
                .Include(s => s.Gym)
                .Include(s => s.TrainerServices)
                    .ThenInclude(ts => ts.Trainer)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Price,
                    s.Duration,
                    s.Category,
                    s.IsActive,
                    Gym = new { s.Gym.Name, s.Gym.Id, s.Gym.Address, s.Gym.Phone },
                    AvailableTrainers = s.TrainerServices
                        .Where(ts => ts.Trainer.IsActive)
                        .Select(ts => new
                        {
                            ts.Trainer.Id,
                            ts.Trainer.FirstName,
                            ts.Trainer.LastName,
                            ts.Trainer.Specialization,
                            ts.Trainer.Experience,
                            ts.Trainer.Bio
                        })
                })
                .FirstOrDefaultAsync();

            if (service == null)
            {
                return NotFound();
            }

            return Ok(service);
        }

        // GET: api/services/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<object>>> GetServiceCategories()
        {
            var categories = await _context.Services
                .Where(s => s.IsActive)
                .GroupBy(s => s.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    ServiceCount = g.Count(),
                    AveragePrice = g.Average(s => s.Price),
                    MinPrice = g.Min(s => s.Price),
                    MaxPrice = g.Max(s => s.Price)
                })
                .OrderBy(c => c.Category)
                .ToListAsync();

            return Ok(categories);
        }
    }
}