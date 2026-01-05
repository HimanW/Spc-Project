namespace Spc.Client.Models;

public sealed class CartItem
{
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}