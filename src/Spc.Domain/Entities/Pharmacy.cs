using Spc.Domain.Enums;

namespace Spc.Domain.Entities;

public class Pharmacy
{
    public Guid PharmacyId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public PharmacyType Type { get; set; }
    public string RegNo { get; set; } = default!;
    public string Address { get; set; } = default!;
    public PharmacyStatus Status { get; set; } = PharmacyStatus.ACTIVE;
}
