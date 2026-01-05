using Spc.Application.Common;

namespace Spc.Application.Drugs;

public interface IDrugService
{
    Task<PagedResult<DrugDto>> SearchAsync(string? query, string? category, int page, int pageSize, CancellationToken ct);
    Task<DrugDto?> GetByIdAsync(Guid drugId, CancellationToken ct);
}
