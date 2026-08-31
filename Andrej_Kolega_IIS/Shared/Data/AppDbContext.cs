using Andrej_Kolega_IIS.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Andrej_Kolega_IIS.Shared.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
                entity.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(200);
                entity.Property(o => o.ShippingCity).IsRequired().HasMaxLength(200);
                entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasMany(o => o.Items)
                    .WithOne(i => i.Order)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(t => t.TokenHash).IsUnique();
                entity.Property(t => t.TokenHash).IsRequired().HasMaxLength(200);

                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
