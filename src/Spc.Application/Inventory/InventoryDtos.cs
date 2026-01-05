namespace Spc.Application.Inventory;
public sealed class InventoryUpdateLineDto
{
    public required Guid DrugId { get; init; }
    public required int QuantityChange { get; init; }
    public required string Reason { get; init; }
}
public sealed class InventoryUpdateBatchRequest
{
    public required Guid LocationId { get; init; }
    public required string SourceType { get; init; }
    public required List<InventoryUpdateLineDto> Updates { get; init; }
}
public sealed class InventoryBalanceDto
{
    public required Guid LocationId { get; init; }
    public required Guid DrugId { get; init; }
    public required string DrugName { get; init; }
    public required int QuantityOnHand { get; init; }
}