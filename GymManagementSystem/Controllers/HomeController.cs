using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Models;

namespace GymManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var trainers = await _context.Trainers
            .Where(t => t.IsActive)
            .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.Service)
            .Take(3)
            .ToListAsync();

        var services = await _context.Services
            .Where(s => s.IsActive)
            .Take(6)
            .ToListAsync();

        ViewBag.Trainers = trainers;
        ViewBag.Services = services;
        
        return View();
    }

    public async Task<IActionResult> Trainers()
    {
        var trainers = await _context.Trainers
            .Where(t => t.IsActive)
            .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.Service)
            .ToListAsync();

        return View(trainers);
    }

    public async Task<IActionResult> Services()
    {
        var services = await _context.Services
            .Where(s => s.IsActive)
            .Include(s => s.TrainerServices)
                .ThenInclude(ts => ts.Trainer)
            .ToListAsync();

        return View(services);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
