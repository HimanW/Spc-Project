namespace Spc.Application.Pharmacies;
public interface IPharmacyService
{
    Task<PharmacyDto> CreateAsync(CreatePharmacyRequest request, CancellationToken ct);
    Task<PharmacyDto?> GetByIdAsync(Guid pharmacyId, CancellationToken ct);
}