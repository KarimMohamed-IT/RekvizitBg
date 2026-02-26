using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RekvizitBg.Data;
using RekvizitBg.Models;

namespace RekvizitBg.Pages
{
    public class CatalogModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CatalogModel> _logger;

        public CatalogModel(AppDbContext context, ILogger<CatalogModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Category> Categories { get; set; } = new();
        public List<Product> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SubcategoryId { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["ActivePage"] = "Catalog";

            Categories = await _context.Categories
                .Include(c => c.Subcategories)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .AsQueryable();

            if (CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == CategoryId.Value);
            }
            if (SubcategoryId.HasValue)
            {
                query = query.Where(p => p.SubcategoryId == SubcategoryId.Value);
            }

            Products = await query.OrderBy(p => p.SortOrder).ToListAsync();
        }
    }
}
