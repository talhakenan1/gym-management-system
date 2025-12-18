using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymManagementSystem.Models;

namespace GymManagementSystem.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PersonalDataModel> _logger;

        public PersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<PersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kullanıcı bulunamadı: '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDownloadPersonalDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kullanıcı bulunamadı: '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("Kullanıcı kişisel verilerini indirdi. UserId: {UserId}", _userManager.GetUserId(User));

            // Only include personal data for download
            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.TryAdd(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            // Add identity properties (using TryAdd to avoid duplicate key errors)
            personalData.TryAdd("Email", user.Email ?? "");
            personalData.TryAdd("UserName", user.UserName ?? "");
            personalData.TryAdd("FirstName", user.FirstName);
            personalData.TryAdd("LastName", user.LastName);
            personalData.TryAdd("PhoneNumber", user.PhoneNumber ?? "");

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.TryAdd($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            Response.Headers.Append("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(personalData, new JsonSerializerOptions { WriteIndented = true })), 
                "application/json");
        }
    }
}

