using Spc.Domain.Enums;
namespace Spc.Application.Pharmacies;
public sealed class CreatePharmacyRequest
{
    public required string Name { get; init; }
    public required PharmacyType Type { get; init; }
    public required string RegNo { get; init; }
    public required string Address { get; init; }
}
public sealed class PharmacyDto
{
    public required Guid PharmacyId { get; init; }
    public required string Name { get; init; }
    public required PharmacyType Type { get; init; }
    public required string RegNo { get; init; }
    public required string Address { get; init; }
}