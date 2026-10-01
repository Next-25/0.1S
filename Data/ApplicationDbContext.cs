using _01S.Model;
using Microsoft.EntityFrameworkCore;

namespace _01S.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentLine> DocumentLines => Set<DocumentLine>();
        public DbSet<Stock> Stocks => Set<Stock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Двусторонняя связь Document <-> DocumentLine
            modelBuilder.Entity<Document>()
                .HasMany(d => d.Lines)
                .WithOne(l => l.Document) // Указываем обратное навигационное свойство!
                .HasForeignKey(l => l.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка связи DocumentLine -> Product
            modelBuilder.Entity<DocumentLine>()
                .HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
                .HasIndex(s => s.ProductId)
                .IsUnique();
        }
    }
}
