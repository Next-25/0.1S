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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связи Document -> DocumentLines (Один-ко-многим)
            modelBuilder
                .Entity<Document>()
                .HasMany(d => d.Lines)
                .WithOne()
                .HasForeignKey(l => l.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка связи DocumentLine -> Product (Многие-к-одному)
            modelBuilder
                .Entity<DocumentLine>()
                .HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
