using CampusEats.Features.Loyalty;
using CampusEats.Features.Menu;
using CampusEats.Features.Orders;
using CampusEats.Features.Payment;
using CampusEats.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Persistence;

public class CampusEatsContext(DbContextOptions<CampusEatsContext> options) : DbContext(options)
{
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<UserLoyalty> UserLoyalties { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.UserId).IsRequired();
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.Status).HasConversion<string>();
            
            // Configure one-to-many relationship with OrderItems
            entity.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.MenuItemName).IsRequired();
            entity.Property(oi => oi.Price).HasPrecision(18, 2);
        });

        // Configure Payment entity
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Status).HasConversion<string>();
            entity.Property(p => p.UserId).IsRequired();
            entity.Property(p => p.StripeSessionId).IsRequired();
            entity.HasIndex(p => p.StripeSessionId);
            entity.HasIndex(p => p.StripePaymentIntentId);
            entity.HasIndex(p => p.OrderId);
        });

        // Configure UserLoyalty entity
        modelBuilder.Entity<UserLoyalty>(entity =>
        {
            entity.HasKey(ul => ul.Id);
            entity.Property(ul => ul.UserId).IsRequired();
            entity.HasIndex(ul => ul.UserId).IsUnique();
        });
    }
}
