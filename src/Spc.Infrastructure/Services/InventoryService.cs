using Microsoft.EntityFrameworkCore;
using Spc.Application.Inventory;
using Spc.Domain.Entities;
using Spc.Domain.Enums;
using Spc.Infrastructure.Data;
using Spc.Application.Common.Exceptions;

namespace Spc.Infrastructure.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly SpcDbContext _db;
    public InventoryService(SpcDbContext db) => _db = db;

    public async Task ApplyUpdatesAsync(InventoryUpdateBatchRequest request, string createdBy, CancellationToken ct)
    {
        if (request.Updates is null || request.Updates.Count == 0) 
            throw new ValidationException("No inventory updates provided.");

        var locationExists = await _db.Locations.AnyAsync(l => l.LocationId == request.LocationId, ct);
        if (!locationExists) 
            throw new NotFoundException("Location not found.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        foreach (var line in request.Updates)
        {
            if (line.QuantityChange == 0) continue;
            var drugExists = await _db.Drugs.AnyAsync(d => d.DrugId == line.DrugId && d.IsActive, ct);
            if (!drugExists) 
                throw new NotFoundException($"Drug not found/inactive: {line.DrugId}");

            var balance = await _db.InventoryBalances.FirstOrDefaultAsync(b => b.LocationId == request.LocationId && b.DrugId == line.DrugId, ct);
            if (balance is null)
            {
                balance = new InventoryBalance { LocationId = request.LocationId, DrugId = line.DrugId, QuantityOnHand = 0 };
                _db.InventoryBalances.Add(balance);
            }

            if (balance.QuantityOnHand + line.QuantityChange < 0) 
                throw new ConflictException("Stock cannot go below zero.");

            balance.QuantityOnHand += line.QuantityChange;

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                LocationId = request.LocationId,
                DrugId = line.DrugId,
                QtyChange = line.QuantityChange,
                Reason = line.Reason.Trim(),
                CreatedBy = createdBy
            });
        }
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<List<InventoryBalanceDto>> GetBalancesAsync(Guid? locationId, Guid? drugId, CancellationToken ct)
    {
        var q = _db.InventoryBalances.AsNoTracking().Include(b => b.Drug).AsQueryable();
        if (locationId.HasValue) q = q.Where(b => b.LocationId == locationId.Value);
        if (drugId.HasValue) q = q.Where(b => b.DrugId == drugId.Value);
        return await q.OrderBy(b => b.Drug.Name)
            .Select(b => new InventoryBalanceDto { LocationId = b.LocationId, DrugId = b.DrugId, DrugName = b.Drug.Name, QuantityOnHand = b.QuantityOnHand })
            .ToListAsync(ct);
    }

    public async Task<Guid> GetDefaultWarehouseLocationIdAsync(CancellationToken ct)
    {
        var wh = await _db.Locations.AsNoTracking().Where(l => l.Type == LocationType.WAREHOUSE).Select(l => l.LocationId).FirstOrDefaultAsync(ct);
        if (wh == Guid.Empty) throw new InvalidOperationException("No warehouse location configured.");
        return wh;
    }

    public async Task ReserveStockAsync(Guid locationId, List<(Guid drugId, int qty)> items, string reference, string createdBy, CancellationToken ct)
    {
        foreach (var (drugId, qty) in items)
        {
            var balance = await _db.InventoryBalances.FirstOrDefaultAsync(b => b.LocationId == locationId && b.DrugId == drugId, ct);
            if (balance is null || balance.QuantityOnHand < qty) 
                throw new ConflictException("Insufficient stock to reserve.");

            balance.QuantityOnHand -= qty;
            _db.InventoryTransactions.Add(new InventoryTransaction { LocationId = locationId, DrugId = drugId, QtyChange = -qty, Reason = "RESERVED_FOR_ORDER", Reference = reference, CreatedBy = createdBy });
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReleaseReservedStockAsync(Guid locationId, List<(Guid drugId, int qty)> items, string reference, string createdBy, CancellationToken ct)
    {
        foreach (var (drugId, qty) in items)
        {
            var balance = await _db.InventoryBalances.FirstOrDefaultAsync(b => b.LocationId == locationId && b.DrugId == drugId, ct);
            if (balance is null) { balance = new InventoryBalance { LocationId = locationId, DrugId = drugId, QuantityOnHand = 0 }; _db.InventoryBalances.Add(balance); }
            balance.QuantityOnHand += qty;
            _db.InventoryTransactions.Add(new InventoryTransaction { LocationId = locationId, DrugId = drugId, QtyChange = qty, Reason = "RELEASED_ORDER_RESERVATION", Reference = reference, CreatedBy = createdBy });
        }
        await _db.SaveChangesAsync(ct);
    }
}