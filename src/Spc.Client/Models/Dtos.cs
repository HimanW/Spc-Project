namespace Spc.Client.Services;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; } = new List<T>();
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
}

public sealed class DrugDto
{
    public required Guid DrugId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required decimal UnitPrice { get; init; }
    public required bool IsActive { get; init; }
}

public sealed class CreatePharmacyRequest
{
    public required string Name { get; init; }
    public required int Type { get; init; }   // 1=SPC, 2=DEALER
    public required string RegNo { get; init; }
    public required string Address { get; init; }
}

public sealed class PharmacyDto
{
    public required Guid PharmacyId { get; init; }
    public required string Name { get; init; }
    public required int Type { get; init; }
    public required string RegNo { get; init; }
    public required string Address { get; init; }
}

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
    public required int Status { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public string? Notes { get; init; }
    public required List<OrderItemDto> Items { get; init; }
    public required decimal TotalAmount { get; init; }
}

public sealed class UpdateOrderStatusRequest
{
    public required int Status { get; init; } 
}

public enum OrderStatus
{
    PLACED = 1,
    APPROVED = 2,
    DISPATCHED = 3,
    DELIVERED = 4,
    CANCELLED = 5
}