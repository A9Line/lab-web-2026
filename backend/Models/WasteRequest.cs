namespace backend.Models;

public class WasteRequest
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public WasteRequestStatus Status { get; set; }
    public int LaboratoryId { get; set; }
    public int WasteTypeId { get; set; }
    public int CreatedByUserId { get; set; }
    public int? AssigneeUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
