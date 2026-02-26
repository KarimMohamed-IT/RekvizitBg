using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RekvizitBg.Services;
using System.ComponentModel.DataAnnotations;

namespace RekvizitBg.Pages
{
    public class ContactsModel : PageModel
    {
        private readonly ILogger<ContactsModel> _logger;
        private readonly EmailService _emailService;
        private readonly RateLimiterService _rateLimiter;

        // ===== FORM FIELDS (with validation) =====
        [BindProperty]
        [Required(ErrorMessage = "Моля, въведете вашето име.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Името трябва да е между 2 и 100 символа.")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
        [EmailAddress(ErrorMessage = "Моля, въведете валиден имейл адрес.")]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Моля, въведете телефонен номер.")]
        [Phone(ErrorMessage = "Моля, въведете валиден телефонен номер.")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [BindProperty]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Моля, въведете вашето съобщение.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Съобщението трябва да е между 10 и 2000 символа.")]
        public string Message { get; set; } = string.Empty;

        // ===== HONEYPOT FIELD (invisible to real users, filled by bots) =====
        [BindProperty]
        public string Website { get; set; } = string.Empty;

        // ===== TIMING FIELD (form render timestamp — bots submit too fast) =====
        [BindProperty]
        public string FormToken { get; set; } = string.Empty;

        public string GeneratedToken { get; set; } = string.Empty;

        public ContactsModel(ILogger<ContactsModel> logger, EmailService emailService, RateLimiterService rateLimiter)
        {
            _logger = logger;
            _emailService = emailService;
            _rateLimiter = rateLimiter;
        }

        public void OnGet()
        {
            ViewData["ActivePage"] = "Contacts";
            // Generate a time-based token for bot detection
            GeneratedToken = GenerateFormToken();
        }

        public async Task<IActionResult> OnPostSendMessage()
        {
            ViewData["ActivePage"] = "Contacts";

            // ===== SECURITY CHECK 1: Honeypot =====
            if (!string.IsNullOrEmpty(Website))
            {
                _logger.LogWarning("Bot detected (honeypot triggered) from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                // Fake success to not tip off the bot
                TempData["SuccessMessage"] = "Вашето съобщение беше изпратено успешно!";
                return RedirectToPage("/Contacts");
            }

            // ===== SECURITY CHECK 2: Rate limiting =====
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (!_rateLimiter.IsAllowed(clientIp))
            {
                _logger.LogWarning("Rate limit exceeded for IP: {IP}", clientIp);
                TempData["ErrorMessage"] = "Прекалено много заявки. Моля, опитайте отново след минута.";
                return RedirectToPage("/Contacts");
            }

            // ===== SECURITY CHECK 3: Timing (bots submit too fast) =====
            if (!ValidateFormToken(FormToken))
            {
                _logger.LogWarning("Bot detected (timing check failed) from IP: {IP}", clientIp);
                TempData["SuccessMessage"] = "Вашето съобщение беше изпратено успешно!";
                return RedirectToPage("/Contacts");
            }

            // ===== SECURITY CHECK 4: Model validation =====
            if (!ModelState.IsValid)
            {
                GeneratedToken = GenerateFormToken();
                return Page();
            }

            // ===== SECURITY CHECK 5: Input sanitization =====
            var sanitizedName = InputSanitizer.SanitizeText(Name, 100);
            var sanitizedEmail = InputSanitizer.SanitizeEmail(Email);
            var sanitizedPhone = InputSanitizer.SanitizePhone(Phone);
            var sanitizedSubject = InputSanitizer.SanitizeText(Subject, 200);
            var sanitizedMessage = InputSanitizer.SanitizeText(Message, 2000);

            if (string.IsNullOrEmpty(sanitizedEmail))
            {
                ModelState.AddModelError("Email", "Невалиден имейл адрес.");
                GeneratedToken = GenerateFormToken();
                return Page();
            }

            // ===== SECURITY CHECK 6: SQL injection detection =====
            if (InputSanitizer.ContainsSqlInjection(Name) ||
                InputSanitizer.ContainsSqlInjection(Message) ||
                InputSanitizer.ContainsSqlInjection(Subject))
            {
                _logger.LogWarning("Potential SQL injection attempt from IP: {IP}, Name: {Name}", clientIp, Name);
                TempData["ErrorMessage"] = "Невалидно съдържание.";
                return RedirectToPage("/Contacts");
            }

            // ===== SEND EMAIL =====
            try
            {
                var subject = string.IsNullOrEmpty(sanitizedSubject)
                    ? "Запитване от контактна форма"
                    : sanitizedSubject;

                var success = await _emailService.SendContactFormAsync(
                    sanitizedName,
                    sanitizedEmail,
                    sanitizedPhone,
                    subject,
                    sanitizedMessage);

                if (success)
                {
                    _logger.LogInformation("Contact form submitted successfully by {Email} from IP: {IP}", sanitizedEmail, clientIp);
                    TempData["SuccessMessage"] = "Вашето съобщение беше изпратено успешно! Ще се свържем с вас скоро.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Възникна грешка при изпращането. Моля, опитайте отново или се свържете по телефона.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Contact form submission failed for {Email}", sanitizedEmail);
                TempData["ErrorMessage"] = "Възникна грешка. Моля, опитайте отново по-късно.";
            }

            return RedirectToPage("/Contacts");
        }

        // ===== HELPERS =====

        /// <summary>
        /// Generates a time-based token. Bots that submit forms instantly will fail the check.
        /// </summary>
        private static string GenerateFormToken()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // Simple obfuscation — reverse the timestamp string
            var tokenStr = timestamp.ToString();
            var reversed = new string(tokenStr.Reverse().ToArray());
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(reversed));
        }

        /// <summary>
        /// Validates a form token to ensure at least 3 seconds have passed since form render.
        /// </summary>
        private static bool ValidateFormToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var reversed = new string(decoded.Reverse().ToArray());

                if (!long.TryParse(reversed, out var timestamp))
                    return false;

                var formTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                var elapsed = DateTimeOffset.UtcNow - formTime;

                // Must be at least 3 seconds old and no more than 1 hour old
                return elapsed.TotalSeconds >= 3 && elapsed.TotalHours <= 1;
            }
            catch
            {
                return false;
            }
        }
    }
}
