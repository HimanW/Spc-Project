using Microsoft.EntityFrameworkCore;
using Spc.Application.Common.Exceptions;
using Spc.Application.Inventory;
using Spc.Infrastructure.Services;
using Spc.Tests.Helpers;
using Xunit;

namespace Spc.Tests;

public class InventoryServiceTests
{
    [Fact]
    public async Task ReserveStock_WhenEnough_DecreasesBalance()
    {
        using var f = new TestDbFactory();
        var (whId, _, drug1Id, _) = await f.SeedCoreAsync();

        var inv = new InventoryService(f.Db);

        await inv.ReserveStockAsync(whId, new List<(Guid, int)> { (drug1Id, 10) }, "ORDER-1", "tester", default);

        var bal = await f.Db.InventoryBalances.AsNoTracking()
            .FirstAsync(b => b.LocationId == whId && b.DrugId == drug1Id);

        Assert.Equal(90, bal.QuantityOnHand);

        var txn = await f.Db.InventoryTransactions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Reference == "ORDER-1" && t.Reason == "RESERVED_FOR_ORDER");

        Assert.NotNull(txn);
        Assert.Equal(-10, txn!.QtyChange);
    }

    [Fact]
    public async Task ReserveStock_WhenInsufficient_ThrowsConflict()
    {
        using var f = new TestDbFactory();
        var (whId, _, drug1Id, _) = await f.SeedCoreAsync();

        var inv = new InventoryService(f.Db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            inv.ReserveStockAsync(whId, new List<(Guid, int)> { (drug1Id, 9999) }, "ORDER-2", "tester", default));
    }
}
