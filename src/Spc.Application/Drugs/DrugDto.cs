namespace Spc.Application.Drugs;

public sealed class DrugDto
{
    public required Guid DrugId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required decimal UnitPrice { get; init; }
    public required bool IsActive { get; init; }
}
