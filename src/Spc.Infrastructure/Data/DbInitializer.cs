using Microsoft.EntityFrameworkCore;
using Spc.Domain.Entities;
using Spc.Domain.Enums;
namespace Spc.Infrastructure.Data;
public static class DbInitializer
{
    public static async Task SeedAsync(SpcDbContext db)
    {
        await db.Database.MigrateAsync();
        if (!await db.Drugs.AnyAsync()) {
            db.Drugs.AddRange(
                new Drug { Code = "DRG-0001", Name = "Paracetamol 500mg", Category = "Pain Relief", UnitPrice = 12.50m },
                new Drug { Code = "DRG-0002", Name = "Amoxicillin 250mg", Category = "Antibiotic", UnitPrice = 35.00m },
                new Drug { Code = "DRG-0003", Name = "Cetirizine 10mg", Category = "Allergy", UnitPrice = 18.00m }
            );
            await db.SaveChangesAsync();
        }
        if (!await db.Locations.AnyAsync(l => l.Type == LocationType.WAREHOUSE)) {
            db.Locations.Add(new Location { Name = "Main Warehouse", Type = LocationType.WAREHOUSE, Address = "Colombo" });
            await db.SaveChangesAsync();
        }
        if (!await db.Pharmacies.AnyAsync()) {
            db.Pharmacies.Add(new Pharmacy { Name = "SPC Pharmacy - Colombo", Type = PharmacyType.SPC, RegNo = "SPC-PH-0001", Address = "Colombo", Status = PharmacyStatus.ACTIVE });
            await db.SaveChangesAsync();
        }
        // Seed Stock
        var whId = await db.Locations.Where(l => l.Type == LocationType.WAREHOUSE).Select(l => l.LocationId).FirstAsync();
        var drugIds = await db.Drugs.Select(d => d.DrugId).ToListAsync();
        foreach (var drugId in drugIds) {
            if (!await db.InventoryBalances.AnyAsync(b => b.LocationId == whId && b.DrugId == drugId)) {
                db.InventoryBalances.Add(new InventoryBalance { LocationId = whId, DrugId = drugId, QuantityOnHand = 500 });
                db.InventoryTransactions.Add(new InventoryTransaction { LocationId = whId, DrugId = drugId, QtyChange = 500, Reason = "INITIAL_STOCK", CreatedBy = "system" });
            }
        }
        await db.SaveChangesAsync();
    }
}