using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RekvizitBg.Data;
using RekvizitBg.Models;

namespace RekvizitBg.Pages
{
    public class AdminModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdminModel> _logger;

        public AdminModel(AppDbContext context, IWebHostEnvironment env, ILogger<AdminModel> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        public List<Category> Categories { get; set; } = new();
        public List<Subcategory> Subcategories { get; set; } = new();
        public List<Product> Products { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        // ===== CATEGORY ACTIONS =====
        public async Task<IActionResult> OnPostCreateCategoryAsync(string name, string description, IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Името на категорията е задължително.";
                return RedirectToPage();
            }

            var category = new Category
            {
                Name = name,
                Description = description,
                SortOrder = await _context.Categories.CountAsync() + 1
            };

            if (image != null)
            {
                category.ImagePath = await SaveImageAsync(image, "categories");
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Категория \"{name}\" беше създадена успешно!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Категорията беше изтрита.";
            }
            return RedirectToPage();
        }

        // ===== SUBCATEGORY ACTIONS =====
        public async Task<IActionResult> OnPostCreateSubcategoryAsync(string name, string description, int categoryId, IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(name) || categoryId <= 0)
            {
                TempData["Error"] = "Името и категорията са задължителни.";
                return RedirectToPage();
            }

            var subcategory = new Subcategory
            {
                Name = name,
                Description = description,
                CategoryId = categoryId,
                SortOrder = await _context.Subcategories.Where(s => s.CategoryId == categoryId).CountAsync() + 1
            };

            if (image != null)
            {
                subcategory.ImagePath = await SaveImageAsync(image, "subcategories");
            }

            _context.Subcategories.Add(subcategory);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Подкатегория \"{name}\" беше създадена успешно!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteSubcategoryAsync(int id)
        {
            var sub = await _context.Subcategories.FindAsync(id);
            if (sub != null)
            {
                _context.Subcategories.Remove(sub);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Подкатегорията беше изтрита.";
            }
            return RedirectToPage();
        }

        // ===== PRODUCT ACTIONS =====
        public async Task<IActionResult> OnPostCreateProductAsync(string name, string description, int categoryId, int? subcategoryId, IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(name) || categoryId <= 0)
            {
                TempData["Error"] = "Името и категорията са задължителни.";
                return RedirectToPage();
            }

            var product = new Product
            {
                Name = name,
                Description = description,
                CategoryId = categoryId,
                SubcategoryId = subcategoryId > 0 ? subcategoryId : null,
                SortOrder = await _context.Products.Where(p => p.CategoryId == categoryId).CountAsync() + 1
            };

            if (image != null)
            {
                product.ImagePath = await SaveImageAsync(image, "products");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Продукт \"{name}\" беше добавен успешно!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Продуктът беше изтрит.";
            }
            return RedirectToPage();
        }

        // ===== HELPERS =====
        private async Task LoadDataAsync()
        {
            Categories = await _context.Categories
                .Include(c => c.Subcategories)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            Subcategories = await _context.Subcategories
                .Include(s => s.Category)
                .OrderBy(s => s.Category.Name).ThenBy(s => s.SortOrder)
                .ToListAsync();

            Products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        private async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsDir);

            var uniqueName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folder}/{uniqueName}";
        }
    }
}
