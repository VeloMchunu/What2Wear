using Microsoft.EntityFrameworkCore;

namespace What2Wear.Web.Data
{
    public class What2WearDbContext : DbContext
    {
        public What2WearDbContext(DbContextOptions<What2WearDbContext> options) : base(options)
        {
        }

        public DbSet<Models.User> Users { get; set; } = null!;
        public DbSet<Models.Photo> Photos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<Models.User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(256);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasMany(e => e.Photos).WithOne(p => p.User).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // Photo configuration
            modelBuilder.Entity<Models.Photo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(512);
                entity.Property(e => e.FileUrl).IsRequired();
                entity.Property(e => e.ClothingType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Tags).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
