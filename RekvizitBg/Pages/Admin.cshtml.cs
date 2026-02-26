using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RekvizitBg.Data;
using RekvizitBg.Models;
using RekvizitBg.Services;

namespace RekvizitBg.Pages
{
    public class AdminModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdminModel> _logger;

        // Allowed extensions for upload security
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

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
        public async Task<IActionResult> OnPostCreateCategoryAsync(string name, string? description, IFormFile? image)
        {
            // Sanitize inputs
            name = InputSanitizer.SanitizeText(name, 100);
            description = InputSanitizer.SanitizeText(description, 500);

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
                var imagePath = await SaveImageSecureAsync(image, "categories");
                if (imagePath == null)
                {
                    TempData["Error"] = "Невалиден файл. Позволени са само изображения до 5MB.";
                    return RedirectToPage();
                }
                category.ImagePath = imagePath;
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Category created: {Name}", name);
            TempData["Success"] = $"Категория \"{name}\" беше създадена успешно!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Delete associated image file
                DeleteImageFile(category.ImagePath);
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Category deleted: {Id} - {Name}", id, category.Name);
                TempData["Success"] = "Категорията беше изтрита.";
            }
            return RedirectToPage();
        }

        // ===== SUBCATEGORY ACTIONS =====
        public async Task<IActionResult> OnPostCreateSubcategoryAsync(string name, string? description, int categoryId, IFormFile? image)
        {
            name = InputSanitizer.SanitizeText(name, 100);
            description = InputSanitizer.SanitizeText(description, 500);

            if (string.IsNullOrWhiteSpace(name) || categoryId <= 0)
            {
                TempData["Error"] = "Името и категорията са задължителни.";
                return RedirectToPage();
            }

            // Verify category exists (prevent ID manipulation)
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                TempData["Error"] = "Избраната категория не съществува.";
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
                var imagePath = await SaveImageSecureAsync(image, "subcategories");
                if (imagePath == null)
                {
                    TempData["Error"] = "Невалиден файл.";
                    return RedirectToPage();
                }
                subcategory.ImagePath = imagePath;
            }

            _context.Subcategories.Add(subcategory);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Subcategory created: {Name} in category {CategoryId}", name, categoryId);
            TempData["Success"] = $"Подкатегория \"{name}\" беше създадена успешно!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteSubcategoryAsync(int id)
        {
            var sub = await _context.Subcategories.FindAsync(id);
            if (sub != null)
            {
                DeleteImageFile(sub.ImagePath);
                _context.Subcategories.Remove(sub);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Subcategory deleted: {Id}", id);
                TempData["Success"] = "Подкатегорията беше изтрита.";
            }
            return RedirectToPage();
        }

        // ===== PRODUCT ACTIONS =====
        public async Task<IActionResult> OnPostCreateProductAsync(string name, string? description, int categoryId, int? subcategoryId, IFormFile? image)
        {
            name = InputSanitizer.SanitizeText(name, 200);
            description = InputSanitizer.SanitizeText(description, 1000);

            if (string.IsNullOrWhiteSpace(name) || categoryId <= 0)
            {
                TempData["Error"] = "Името и категорията са задължителни.";
                return RedirectToPage();
            }

            // Verify category exists
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                TempData["Error"] = "Избраната категория не съществува.";
                return RedirectToPage();
            }

            // Verify subcategory if provided
            if (subcategoryId.HasValue && subcategoryId > 0)
            {
                var subExists = await _context.Subcategories.AnyAsync(s => s.Id == subcategoryId.Value && s.CategoryId == categoryId);
                if (!subExists)
                {
                    TempData["Error"] = "Избраната подкатегория не съществува.";
                    return RedirectToPage();
                }
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
                var imagePath = await SaveImageSecureAsync(image, "products");
                if (imagePath == null)
                {
                    TempData["Error"] = "Невалиден файл.";
                    return RedirectToPage();
                }
                product.ImagePath = imagePath;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product created: {Name} in category {CategoryId}", name, categoryId);
            TempData["Success"] = $"Продукт \"{name}\" беше добавен успешно!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                DeleteImageFile(product.ImagePath);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product deleted: {Id}", id);
                TempData["Success"] = "Продуктът беше изтрит.";
            }
            return RedirectToPage();
        }

        // ===== SECURE HELPERS =====
        private async Task LoadDataAsync()
        {
            Categories = await _context.Categories
                .Include(c => c.Subcategories)
                .OrderBy(c => c.SortOrder)
                .AsNoTracking()  // Performance: no change tracking for read-only
                .ToListAsync();

            Subcategories = await _context.Subcategories
                .Include(s => s.Category)
                .OrderBy(s => s.Category.Name).ThenBy(s => s.SortOrder)
                .AsNoTracking()
                .ToListAsync();

            Products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Subcategory)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Securely saves an uploaded image with validation.
        /// Returns null if the file is invalid.
        /// </summary>
        private async Task<string?> SaveImageSecureAsync(IFormFile file, string folder)
        {
            // 1. Validate file size
            if (file.Length == 0 || file.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning("File upload rejected: size {Size} bytes", file.Length);
                return null;
            }

            // 2. Validate extension
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                _logger.LogWarning("File upload rejected: extension {Extension}", ext);
                return null;
            }

            // 3. Validate MIME type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("File upload rejected: MIME type {MimeType}", file.ContentType);
                return null;
            }

            // 4. Validate file signature (magic bytes)
            if (!await IsValidImageSignatureAsync(file))
            {
                _logger.LogWarning("File upload rejected: invalid file signature");
                return null;
            }

            // 5. Generate safe filename (GUID, no user-supplied name)
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsDir);

            var uniqueName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, uniqueName);

            // 6. Prevent path traversal
            var fullPath = Path.GetFullPath(filePath);
            if (!fullPath.StartsWith(Path.GetFullPath(uploadsDir)))
            {
                _logger.LogWarning("Path traversal attempt detected");
                return null;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folder}/{uniqueName}";
        }

        /// <summary>
        /// Check file magic bytes to verify it's actually an image.
        /// </summary>
        private static async Task<bool> IsValidImageSignatureAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var header = new byte[8];
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, 8));
            if (bytesRead < 4) return false;

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
            // PNG: 89 50 4E 47
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
            // GIF: 47 49 46 38
            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38) return true;
            // WEBP: 52 49 46 46 ... 57 45 42 50 (RIFF....WEBP)
            if (bytesRead >= 8 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46) return true;

            return false;
        }

        /// <summary>
        /// Delete an image file from disk when its entity is deleted.
        /// </summary>
        private void DeleteImageFile(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !imagePath.StartsWith("/uploads/"))
                return;

            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Deleted image file: {Path}", imagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete image file: {Path}", imagePath);
            }
        }
    }
}
