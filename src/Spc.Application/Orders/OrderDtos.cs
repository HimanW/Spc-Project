using Spc.Domain.Enums;
namespace Spc.Application.Orders;
public sealed class CreateOrderItemDto
{
    public required Guid DrugId { get; init; }
    public required int Quantity { get; init; }
}
public sealed class CreateOrderRequest
{
    public required Guid PharmacyId { get; init; }
    public required List<CreateOrderItemDto> Items { get; init; }
    public string? Notes { get; init; }
}
public sealed class OrderItemDto
{
    public required Guid DrugId { get; init; }
    public required string DrugName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}
public sealed class OrderDto
{
    public required Guid OrderId { get; init; }
    public required Guid PharmacyId { get; init; }
    public required OrderStatus Status { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public string? Notes { get; init; }
    public required List<OrderItemDto> Items { get; init; }
    public required decimal TotalAmount { get; init; }
}
public sealed class UpdateOrderStatusRequest
{
    public required OrderStatus Status { get; init; }
}