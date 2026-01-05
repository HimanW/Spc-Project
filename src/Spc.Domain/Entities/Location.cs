using Spc.Domain.Enums;

namespace Spc.Domain.Entities;

public class Location
{
    public Guid LocationId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public LocationType Type { get; set; }
    public string Address { get; set; } = default!;
}
