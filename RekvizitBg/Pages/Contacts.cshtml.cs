﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RekvizitBg.Services;

namespace RekvizitBg.Pages
{
    public class ContactsModel : PageModel
    {
        private readonly string htmlNewLine = "<br>";
        //private CreateAssessmentSample assessmentSample;
        
        private readonly ILogger<ContactsModel> _logger;

        [BindProperty]
        public string Name { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Phone { get; set; }
        [BindProperty]
        public string Message { get; set; }

        private readonly EmailService _emailService;

        public ContactsModel(ILogger<ContactsModel> logger, EmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public void OnGet()
        {
            ViewData["ActivePage"] = "Contacts"; // Set the active page here
        }

        public async Task<IActionResult> OnPostSendMessage()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //assessmentSample.createAssessment();


            string messageBody = @$"Name: {Name} {htmlNewLine} Email: {Email} {htmlNewLine} Phone: {Phone} {htmlNewLine} Message: {Message}";

            await _emailService.SendEmailAsync(Email, "Contact Form Submission", messageBody);

            // Логика за обработка на съобщението, като например изпращане на имейл или записване в база данни

            TempData["SuccessMessage"] = "Вашето съобщение беше изпратено успешно!";
            return RedirectToPage("/Contacts");
        }
    }
}
