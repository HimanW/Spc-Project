using Microsoft.EntityFrameworkCore;
using Spc.Application.Common;
using Spc.Application.Drugs;
using Spc.Infrastructure.Data;

namespace Spc.Infrastructure.Services;

public sealed class DrugService : IDrugService
{
    private readonly SpcDbContext _db;

    public DrugService(SpcDbContext db) => _db = db;

    public async Task<PagedResult<DrugDto>> SearchAsync(string? query, string? category, int page, int pageSize, CancellationToken ct)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var q = _db.Drugs.AsNoTracking().Where(d => d.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(d =>
                EF.Functions.Like(d.Name, $"%{term}%") ||
                EF.Functions.Like(d.Code, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim();
            q = q.Where(d => d.Category == cat);
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DrugDto
            {
                DrugId = d.DrugId,
                Code = d.Code,
                Name = d.Name,
                Category = d.Category,
                UnitPrice = d.UnitPrice,
                IsActive = d.IsActive
            })
            .ToListAsync(ct);

        return new PagedResult<DrugDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<DrugDto?> GetByIdAsync(Guid drugId, CancellationToken ct)
    {
        return await _db.Drugs.AsNoTracking()
            .Where(d => d.DrugId == drugId)
            .Select(d => new DrugDto
            {
                DrugId = d.DrugId,
                Code = d.Code,
                Name = d.Name,
                Category = d.Category,
                UnitPrice = d.UnitPrice,
                IsActive = d.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }
}
