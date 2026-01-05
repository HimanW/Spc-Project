namespace Spc.Domain.Entities;

public class Drug
{
    public Guid DrugId { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = default!;     // e.g., DRG-0001
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!; // e.g., Pain Relief

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
