using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Spc.Domain.Entities;
using Spc.Domain.Enums;
using Spc.Infrastructure.Data;

namespace Spc.Tests.Helpers;

public sealed class TestDbFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    public SpcDbContext Db { get; }

    public TestDbFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SpcDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new SpcDbContext(options);
        Db.Database.EnsureCreated();
    }

    public async Task<(Guid warehouseId, Guid pharmacyId, Guid drug1Id, Guid drug2Id)> SeedCoreAsync()
    {
        // Warehouse
        var wh = new Location { Name = "Main Warehouse", Type = LocationType.WAREHOUSE, Address = "Colombo" };
        Db.Locations.Add(wh);

        // Pharmacy
        var ph = new Pharmacy
        {
            Name = "Test Pharmacy",
            Type = PharmacyType.SPC,
            RegNo = "TEST-PH-001",
            Address = "Colombo",
            Status = PharmacyStatus.ACTIVE
        };
        Db.Pharmacies.Add(ph);

        // Drugs
        var d1 = new Drug { Code = "DRG-T1", Name = "Paracetamol 500mg", Category = "Pain Relief", UnitPrice = 12.50m };
        var d2 = new Drug { Code = "DRG-T2", Name = "Amoxicillin 250mg", Category = "Antibiotic", UnitPrice = 35.00m };
        Db.Drugs.AddRange(d1, d2);

        await Db.SaveChangesAsync();

        // Stock
        Db.InventoryBalances.AddRange(
            new InventoryBalance { LocationId = wh.LocationId, DrugId = d1.DrugId, QuantityOnHand = 100 },
            new InventoryBalance { LocationId = wh.LocationId, DrugId = d2.DrugId, QuantityOnHand = 50 }
        );

        await Db.SaveChangesAsync();

        return (wh.LocationId, ph.PharmacyId, d1.DrugId, d2.DrugId);
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
