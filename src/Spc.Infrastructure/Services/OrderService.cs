using Microsoft.EntityFrameworkCore;
using Spc.Application.Inventory;
using Spc.Application.Orders;
using Spc.Domain.Entities;
using Spc.Domain.Enums;
using Spc.Infrastructure.Data;
using Spc.Application.Common.Exceptions;

namespace Spc.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly SpcDbContext _db;
    private readonly IInventoryService _inventory;
    public OrderService(SpcDbContext db, IInventoryService inventory) { _db = db; _inventory = inventory; }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, string createdBy, CancellationToken ct)
    {
        if (request.Items is null || request.Items.Count == 0) 
            throw new ValidationException("Order must contain at least one item.");

        var pharmacy = await _db.Pharmacies.FirstOrDefaultAsync(p => p.PharmacyId == request.PharmacyId, ct);
        
        if (pharmacy is null)
            throw new NotFoundException("Pharmacy not found.");
            
        if (pharmacy.Status != PharmacyStatus.ACTIVE) 
            throw new ValidationException("Pharmacy is not active.");

        var orderId = Guid.NewGuid();
        var warehouseId = await _inventory.GetDefaultWarehouseLocationIdAsync(ct);
        var drugIds = request.Items.Select(i => i.DrugId).Distinct().ToList();
        var drugs = await _db.Drugs.Where(d => drugIds.Contains(d.DrugId) && d.IsActive).ToListAsync(ct);

        if (drugs.Count != drugIds.Count)
            throw new ValidationException("One or more drugs are invalid/inactive.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        await _inventory.ReserveStockAsync(warehouseId, request.Items.Select(i => (i.DrugId, i.Quantity)).ToList(), orderId.ToString(), createdBy, ct);

        var order = new Order { OrderId = orderId, PharmacyId = request.PharmacyId, Status = OrderStatus.PLACED, CreatedBy = createdBy, Notes = request.Notes };
        foreach (var i in request.Items)
        {
            var drug = drugs.Single(d => d.DrugId == i.DrugId);
            order.Items.Add(new OrderItem { DrugId = drug.DrugId, Quantity = i.Quantity, UnitPrice = drug.UnitPrice });
        }
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await GetByIdAsync(orderId, ct) ?? throw new InvalidOperationException("Order failed.");
    }

    public async Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders.AsNoTracking().Include(o => o.Items).ThenInclude(i => i.Drug).FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
        if (order is null) return null;
        var items = order.Items.Select(i => new OrderItemDto { DrugId = i.DrugId, DrugName = i.Drug.Name, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList();
        return new OrderDto { OrderId = order.OrderId, PharmacyId = order.PharmacyId, Status = order.Status, CreatedAtUtc = order.CreatedAtUtc, Notes = order.Notes, Items = items, TotalAmount = items.Sum(x => x.UnitPrice * x.Quantity) };
    }

    public async Task UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, string updatedBy, CancellationToken ct)
{
    var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
    if (order is null) 
        throw new NotFoundException("Order not found.");

    if (order.Status == OrderStatus.CANCELLED)
        throw new ValidationException("Invalid status transition: Order is already cancelled.");

    await using var tx = await _db.Database.BeginTransactionAsync(ct);
    
    if (request.Status == OrderStatus.CANCELLED && (order.Status == OrderStatus.PLACED || order.Status == OrderStatus.APPROVED))
    {
        var warehouseId = await _inventory.GetDefaultWarehouseLocationIdAsync(ct);
        await _inventory.ReleaseReservedStockAsync(warehouseId, order.Items.Select(i => (i.DrugId, i.Quantity)).ToList(), orderId.ToString(), updatedBy, ct);
    }
    
    order.Status = request.Status;
    await _db.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);
}
}