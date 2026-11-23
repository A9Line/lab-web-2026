using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

public sealed record AssignWasteRequestRequest
{
    [Required(ErrorMessage = "Укажите исполнителя")]
    [Range(1, int.MaxValue, ErrorMessage = "assigneeUserId должен быть целым числом больше 0")]
    public int? AssigneeUserId { get; init; }
}
