using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RekvizitBg.Pages
{
    public class ContactsModel : PageModel
    {
        private readonly ILogger<ContactsModel> _logger;

        [BindProperty]
        public string Name { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Message { get; set; }

        public ContactsModel(ILogger<ContactsModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            ViewData["ActivePage"] = "Contacts"; // Set the active page here
        }

        public IActionResult OnPostSendMessage()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Логика за обработка на съобщението, като например изпращане на имейл или записване в база данни

            TempData["SuccessMessage"] = "Вашето съобщение беше изпратено успешно!";
            return RedirectToPage("/Contact");
        }
    }
}
