using System.ComponentModel.DataAnnotations;

namespace RekvizitBg.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(300)]
        public string? ImagePath { get; set; }

        public bool IsAvailable { get; set; } = true;

        public int SortOrder { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int? SubcategoryId { get; set; }
        public Subcategory? Subcategory { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
