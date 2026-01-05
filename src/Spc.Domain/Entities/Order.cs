using Spc.Domain.Enums;

namespace Spc.Domain.Entities;

public class Order
{
    public Guid OrderId { get; set; } = Guid.NewGuid();

    public Guid PharmacyId { get; set; }
    public Pharmacy Pharmacy { get; set; } = default!;

    public OrderStatus Status { get; set; } = OrderStatus.PLACED;

    public string CreatedBy { get; set; } = "system";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
