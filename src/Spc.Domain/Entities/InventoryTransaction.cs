namespace Spc.Domain.Entities;

public class InventoryTransaction
{
    public Guid TxnId { get; set; } = Guid.NewGuid();

    public Guid LocationId { get; set; }
    public Location Location { get; set; } = default!;

    public Guid DrugId { get; set; }
    public Drug Drug { get; set; } = default!;

    public int QtyChange { get; set; } // + add stock, - reduce stock
    public string Reason { get; set; } = default!; // e.g., PURCHASED_STOCK, PLANT_PRODUCTION, RESERVED_FOR_ORDER
    public string? Reference { get; set; } // e.g., OrderId

    public string CreatedBy { get; set; } = "system";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
