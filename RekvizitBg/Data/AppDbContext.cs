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

            // ===== SEED CATEGORIES =====
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Мебели", Description = "Изключителен избор от ретро и модерни мебели — маси, столове, дивани, шкафове и декоративни мебели за всякакви сцени и филмови продукции.", ImagePath = "/images/cat_furniture.png", SortOrder = 1 },
                new Category { Id = 2, Name = "Аксесоари", Description = "Уникални аксесоари — часовници, бижута, очила, чанти, шапки и антични предмети за филми, реклами и театрални проекти.", ImagePath = "/images/cat_accessories.png", SortOrder = 2 },
                new Category { Id = 3, Name = "Оръжия и Брони", Description = "Голям избор от реквизитни мечове, щитове, брони, шлемове, арбалети и огнестрелно оръжие за исторически и кино продукции.", ImagePath = "/images/cat_weapons.png", SortOrder = 3 },
                new Category { Id = 4, Name = "Декорации", Description = "Разнообразие от декоративни елементи — полилеи, свещници, часовници, рамки, вази и антични предмети за филмови и театрални сцени.", ImagePath = "/images/cat_decorations.png", SortOrder = 4 },
                new Category { Id = 5, Name = "Автомобили", Description = "Ретро автомобили, модерни коли, линейки, багери и специални превозни средства за филмови и рекламни продукции.", ImagePath = "/images/cat_vehicles.png", SortOrder = 5 },
                new Category { Id = 6, Name = "Костюми", Description = "Театрални и филмови костюми от различни исторически периоди — рокли, униформи, военни облекла и народни носии.", ImagePath = "/images/costumes.png", SortOrder = 6 },
                new Category { Id = 7, Name = "Каскадьори и Актьори", Description = "Професионални каскадьори и актьори за филмови, рекламни и телевизионни продукции — екшън сцени, трюкове и драматични изпълнения.", ImagePath = "/images/cat_stuntmen.png", SortOrder = 7 }
            );

            // ===== SEED SUBCATEGORIES =====
            modelBuilder.Entity<Subcategory>().HasData(
                // Декорации subcategories
                new Subcategory { Id = 1, Name = "Полилеи", Description = "Барокови и класически полилеи с кристали — от грандиозни дворцови до по-малки декоративни модели за филмови сцени.", ImagePath = "/images/sub_chandeliers.png", CategoryId = 4, SortOrder = 1 },

                // Автомобили subcategories
                new Subcategory { Id = 2, Name = "Ретро Автомобили", Description = "Класически автомобили от 40-те до 80-те години — за филми, реклами и специални събития.", CategoryId = 5, SortOrder = 1 },
                new Subcategory { Id = 3, Name = "Нови Автомобили", Description = "Модерни автомобили — луксозни седани, SUV-та и спортни коли за филмови продукции.", CategoryId = 5, SortOrder = 2 },
                new Subcategory { Id = 4, Name = "Специални Превозни Средства", Description = "Линейки, пожарни, багери, камиони и други специализирани превозни средства за филмови сцени.", CategoryId = 5, SortOrder = 3 }
            );
        }
    }
}
