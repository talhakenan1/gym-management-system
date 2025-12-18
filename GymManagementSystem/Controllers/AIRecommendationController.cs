using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagementSystem.Models;
using GymManagementSystem.Services;
using System.Security.Claims;

namespace GymManagementSystem.Controllers
{
    [Authorize]
    public class AIRecommendationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIService _openAIService;
        private readonly GeminiService _geminiService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public AIRecommendationController(ApplicationDbContext context, OpenAIService openAIService, GeminiService geminiService, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _openAIService = openAIService;
            _geminiService = geminiService;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        // GET: AIRecommendation
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recommendations = await _context.AIRecommendations
                .Where(r => r.UserId == userId && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(recommendations);
        }

        // GET: AIRecommendation/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AIRecommendation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Age,Height,Weight,Gender,FitnessGoal,ActivityLevel,Type")] AIRecommendation aiRecommendation, IFormFile? photo)
        {
            // Remove User validation error
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                aiRecommendation.UserId = userId!;

                try
                {
                    // Handle photo upload if provided
                    if (photo != null && photo.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "photos");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(fileStream);
                        }

                        aiRecommendation.PhotoPath = "/uploads/photos/" + uniqueFileName;
                        
                        // Simple photo analysis (in a real app, you'd use computer vision)
                        aiRecommendation.PhotoAnalysis = await _openAIService.AnalyzePhoto("Kullanıcı fitness fotoğrafı yükledi. Genel vücut kompozisyonu analizi yapılması isteniyor.");
                    }

                    // Determine which AI service to use (Gemini preferred if available)
                    var useGemini = !string.IsNullOrEmpty(_configuration["Gemini:ApiKey"]) && 
                                   _configuration["Gemini:ApiKey"] != "your-gemini-api-key-here";
                    
                    // Generate AI recommendations
                    if (aiRecommendation.Type == RecommendationType.Exercise || aiRecommendation.Type == RecommendationType.Combined)
                    {
                        if (useGemini)
                        {
                            aiRecommendation.ExerciseRecommendations = await _geminiService.GenerateExerciseRecommendations(
                                aiRecommendation.Age ?? 30,
                                aiRecommendation.Height ?? 170,
                                aiRecommendation.Weight ?? 70,
                                aiRecommendation.Gender ?? "Erkek",
                                aiRecommendation.FitnessGoal ?? FitnessGoal.GeneralFitness,
                                aiRecommendation.ActivityLevel ?? ActivityLevel.ModeratelyActive
                            );
                        }
                        else
                        {
                            aiRecommendation.ExerciseRecommendations = await _openAIService.GenerateExerciseRecommendations(
                                aiRecommendation.Age ?? 30,
                                aiRecommendation.Height ?? 170,
                                aiRecommendation.Weight ?? 70,
                                aiRecommendation.Gender ?? "Erkek",
                                aiRecommendation.FitnessGoal ?? FitnessGoal.GeneralFitness,
                                aiRecommendation.ActivityLevel ?? ActivityLevel.ModeratelyActive
                            );
                        }
                    }

                    if (aiRecommendation.Type == RecommendationType.Diet || aiRecommendation.Type == RecommendationType.Combined)
                    {
                        if (useGemini)
                        {
                            aiRecommendation.DietRecommendations = await _geminiService.GenerateDietRecommendations(
                                aiRecommendation.Age ?? 30,
                                aiRecommendation.Height ?? 170,
                                aiRecommendation.Weight ?? 70,
                                aiRecommendation.Gender ?? "Erkek",
                                aiRecommendation.FitnessGoal ?? FitnessGoal.GeneralFitness,
                                aiRecommendation.ActivityLevel ?? ActivityLevel.ModeratelyActive
                            );
                        }
                        else
                        {
                            aiRecommendation.DietRecommendations = await _openAIService.GenerateDietRecommendations(
                                aiRecommendation.Age ?? 30,
                                aiRecommendation.Height ?? 170,
                                aiRecommendation.Weight ?? 70,
                                aiRecommendation.Gender ?? "Erkek",
                                aiRecommendation.FitnessGoal ?? FitnessGoal.GeneralFitness,
                                aiRecommendation.ActivityLevel ?? ActivityLevel.ModeratelyActive
                            );
                        }
                    }

                    // Generate Image if requested
                    if (Request.Form["GenerateImage"] == "on")
                    {
                        string imagePrompt = $"A realistic photo of a {aiRecommendation.Age} year old {aiRecommendation.Gender}, {aiRecommendation.Height}cm height, {aiRecommendation.Weight}kg weight, with a fitness goal of {aiRecommendation.FitnessGoal}. The person looks fit, healthy and happy after achieving their fitness goals. High quality, photorealistic.";
                        aiRecommendation.GeneratedImagePath = await _openAIService.GenerateImage(imagePrompt);
                    }

                    _context.Add(aiRecommendation);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "AI önerileriniz başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(Details), new { id = aiRecommendation.Id });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Öneriler oluşturulurken bir hata oluştu: " + ex.Message;
                }
            }

            return View(aiRecommendation);
        }
        // GET: AIRecommendation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var aiRecommendation = await _context.AIRecommendations
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (aiRecommendation == null)
            {
                return NotFound();
            }

            return View(aiRecommendation);
        }

        // GET: AIRecommendation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var aiRecommendation = await _context.AIRecommendations
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (aiRecommendation == null)
            {
                return NotFound();
            }

            return View(aiRecommendation);
        }

        // POST: AIRecommendation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var aiRecommendation = await _context.AIRecommendations
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (aiRecommendation != null)
            {
                // Delete photo file if exists
                if (!string.IsNullOrEmpty(aiRecommendation.PhotoPath))
                {
                    var photoPath = Path.Combine(_webHostEnvironment.WebRootPath, aiRecommendation.PhotoPath.TrimStart('/'));
                    if (System.IO.File.Exists(photoPath))
                    {
                        System.IO.File.Delete(photoPath);
                    }
                }

                _context.AIRecommendations.Remove(aiRecommendation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}