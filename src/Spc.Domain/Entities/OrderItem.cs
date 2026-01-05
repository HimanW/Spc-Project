namespace Spc.Domain.Entities;

public class OrderItem
{
    public Guid OrderItemId { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public Guid DrugId { get; set; }
    public Drug Drug { get; set; } = default!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
