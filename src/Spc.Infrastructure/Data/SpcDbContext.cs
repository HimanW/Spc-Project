using Microsoft.EntityFrameworkCore;
using Spc.Domain.Entities;
using Spc.Domain.Enums;

namespace Spc.Infrastructure.Data;

public class SpcDbContext : DbContext
{
    public SpcDbContext(DbContextOptions<SpcDbContext> options) : base(options) { }

    public DbSet<Drug> Drugs => Set<Drug>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Drug>(e =>
        {
            e.HasKey(x => x.DrugId);
            e.Property(x => x.Code).IsRequired().HasMaxLength(50);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).IsRequired().HasMaxLength(100);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.HasKey(x => x.LocationId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Address).IsRequired().HasMaxLength(300);
            e.Property(x => x.Type).IsRequired();
            e.HasIndex(x => new { x.Name, x.Type });
        });

        modelBuilder.Entity<InventoryBalance>(e =>
        {
            e.HasKey(x => new { x.LocationId, x.DrugId });
            e.Property(x => x.QuantityOnHand).IsRequired();
            
            // Updated concurrency token configuration
            e.Property(x => x.Version).IsConcurrencyToken();

            e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId);
            e.HasOne(x => x.Drug).WithMany().HasForeignKey(x => x.DrugId);
        });
        modelBuilder.Entity<InventoryTransaction>(e =>
        {
            e.HasKey(x => x.TxnId);
            e.Property(x => x.Reason).IsRequired().HasMaxLength(100);
            e.Property(x => x.Reference).HasMaxLength(100);
            e.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100);
            e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId);
            e.HasOne(x => x.Drug).WithMany().HasForeignKey(x => x.DrugId);
            e.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<Pharmacy>(e =>
        {
            e.HasKey(x => x.PharmacyId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.RegNo).IsRequired().HasMaxLength(100);
            e.Property(x => x.Address).IsRequired().HasMaxLength(300);
            e.Property(x => x.Type).IsRequired();
            e.Property(x => x.Status).IsRequired();
            e.HasIndex(x => x.RegNo).IsUnique();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.OrderId);
            e.Property(x => x.Status).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasOne(x => x.Pharmacy).WithMany().HasForeignKey(x => x.PharmacyId);
            e.HasMany(x => x.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.OrderItemId);
            e.Property(x => x.Quantity).IsRequired();
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.HasOne(x => x.Drug).WithMany().HasForeignKey(x => x.DrugId);
            e.HasIndex(x => new { x.OrderId, x.DrugId });
        });

        base.OnModelCreating(modelBuilder);
    }
}