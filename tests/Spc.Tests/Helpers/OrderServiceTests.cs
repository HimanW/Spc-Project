using Microsoft.EntityFrameworkCore;
using Spc.Application.Common.Exceptions;
using Spc.Application.Inventory;
using Spc.Application.Orders;
using Spc.Infrastructure.Services;
using Spc.Tests.Helpers;
using Xunit;

namespace Spc.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_ReservesStockAndCreatesOrder()
    {
        using var f = new TestDbFactory();
        var (whId, pharmacyId, drug1Id, _) = await f.SeedCoreAsync();

        IInventoryService inv = new InventoryService(f.Db);
        var orders = new OrderService(f.Db, inv);

        var created = await orders.CreateAsync(new CreateOrderRequest
        {
            PharmacyId = pharmacyId,
            Notes = "test",
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { DrugId = drug1Id, Quantity = 5 }
            }
        }, "tester", default);

        Assert.Equal("PLACED", created.Status.ToString());

        // Stock reduced
        var bal = await f.Db.InventoryBalances.AsNoTracking()
            .FirstAsync(b => b.LocationId == whId && b.DrugId == drug1Id);

        Assert.Equal(95, bal.QuantityOnHand);

        // Order saved
        var orderRow = await f.Db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == created.OrderId);
        Assert.NotNull(orderRow);
    }

    [Fact]
    public async Task CancelOrder_ReleasesReservedStock()
    {
        using var f = new TestDbFactory();
        var (whId, pharmacyId, drug1Id, _) = await f.SeedCoreAsync();

        IInventoryService inv = new InventoryService(f.Db);
        var orders = new OrderService(f.Db, inv);

        var created = await orders.CreateAsync(new CreateOrderRequest
        {
            PharmacyId = pharmacyId,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { DrugId = drug1Id, Quantity = 10 }
            }
        }, "tester", default);

        // Cancel
        await orders.UpdateStatusAsync(created.OrderId, new UpdateOrderStatusRequest
        {
            Status = Spc.Domain.Enums.OrderStatus.CANCELLED
        }, "tester", default);

        var bal = await f.Db.InventoryBalances.AsNoTracking()
            .FirstAsync(b => b.LocationId == whId && b.DrugId == drug1Id);

        Assert.Equal(100, bal.QuantityOnHand); // back to original
    }

    [Fact]
    public async Task CreateOrder_WhenInsufficientStock_ThrowsConflict()
    {
        using var f = new TestDbFactory();
        var (_, pharmacyId, drug1Id, _) = await f.SeedCoreAsync();

        IInventoryService inv = new InventoryService(f.Db);
        var orders = new OrderService(f.Db, inv);

        await Assert.ThrowsAsync<ConflictException>(() =>
            orders.CreateAsync(new CreateOrderRequest
            {
                PharmacyId = pharmacyId,
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { DrugId = drug1Id, Quantity = 9999 }
                }
            }, "tester", default));
    }
}
