using backend.Models;

namespace backend.Dtos;

public sealed record WasteRequestResponse(
    int Id,
    string Number,
    string Title,
    string Description,
    WasteRequestStatus Status,
    int LaboratoryId,
    int WasteTypeId,
    int CreatedByUserId,
    int? AssigneeUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static WasteRequestResponse From(WasteRequest item) => new(
        item.Id,
        item.Number,
        item.Title,
        item.Description,
        item.Status,
        item.LaboratoryId,
        item.WasteTypeId,
        item.CreatedByUserId,
        item.AssigneeUserId,
        item.CreatedAt,
        item.UpdatedAt);
}
