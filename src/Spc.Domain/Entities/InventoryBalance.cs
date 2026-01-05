namespace Spc.Domain.Entities;

public class InventoryBalance
{
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;

    public Guid DrugId { get; set; }
    public Drug Drug { get; set; } = default!;

    public int QuantityOnHand { get; set; }

    // Use Guid for concurrency (works on SQLite and SQL Server)
    public Guid Version { get; set; } = Guid.NewGuid();
}