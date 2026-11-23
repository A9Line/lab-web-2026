namespace backend.Models;

public sealed record WasteRequest
{
    public required int Id { get; init; }
    public required string Number { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required WasteRequestStatus Status { get; init; }
    public required int LaboratoryId { get; init; }
    public required int WasteTypeId { get; init; }
    public required int CreatedByUserId { get; init; }
    public int? AssigneeUserId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
