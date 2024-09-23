using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RekvizitBg.Pages
{
    public class AboutUsModel : PageModel
    {
        private readonly ILogger<AboutUsModel> _logger;

        public AboutUsModel(ILogger<AboutUsModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            ViewData["ActivePage"] = "AboutUs"; // Set the active page here

        }
    }
}
