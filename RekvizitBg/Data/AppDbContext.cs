using Microsoft.EntityFrameworkCore;
using RekvizitBg.Models;

namespace RekvizitBg.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subcategory>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Subcategory)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SubcategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Seed initial categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Мебели", Description = "Изключителен избор от ретро и модерни мебели за всякакви сцени и филмови продукции.", ImagePath = "/images/old_furniture.webp", SortOrder = 1 },
                new Category { Id = 2, Name = "Аксесоари", Description = "Уникални аксесоари за разнообразни събития, филми и театрални проекти.", ImagePath = "/images/old_accesoaries.webp", SortOrder = 2 },
                new Category { Id = 3, Name = "Оръжия и Брони", Description = "Голям избор от реквизитни оръжия и брони за исторически и кино продукции.", ImagePath = "/images/old_weapons.webp", SortOrder = 3 },
                new Category { Id = 4, Name = "Декорации", Description = "Разнообразие от декоративни елементи за филмови и театрални сцени.", ImagePath = "/images/old_items.webp", SortOrder = 4 },
                new Category { Id = 5, Name = "Ретро Автомобили", Description = "Класически ретро автомобили под наем за филми, реклами и специални събития.", ImagePath = "/images/retro_cars.png", SortOrder = 5 },
                new Category { Id = 6, Name = "Костюми", Description = "Театрални и филмови костюми от различни исторически периоди.", ImagePath = "/images/costumes.png", SortOrder = 6 }
            );
        }
    }
}
