namespace Spc.Application.Inventory;
public interface IInventoryService
{
    Task ApplyUpdatesAsync(InventoryUpdateBatchRequest request, string createdBy, CancellationToken ct);
    Task<List<InventoryBalanceDto>> GetBalancesAsync(Guid? locationId, Guid? drugId, CancellationToken ct);
    Task ReserveStockAsync(Guid locationId, List<(Guid drugId, int qty)> items, string reference, string createdBy, CancellationToken ct);
    Task ReleaseReservedStockAsync(Guid locationId, List<(Guid drugId, int qty)> items, string reference, string createdBy, CancellationToken ct);
    Task<Guid> GetDefaultWarehouseLocationIdAsync(CancellationToken ct);
}