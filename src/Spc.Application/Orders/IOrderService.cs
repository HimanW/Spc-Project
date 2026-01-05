namespace Spc.Application.Orders;
public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderRequest request, string createdBy, CancellationToken ct);
    Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken ct);
    Task UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, string updatedBy, CancellationToken ct);
}