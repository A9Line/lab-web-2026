using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

// id, number, status и даты клиент не присылает, их задаёт сервер
public sealed record CreateWasteRequestRequest
{
    [Required(ErrorMessage = "Укажите заголовок")]
    [StringLength(80, MinimumLength = 5, ErrorMessage = "Заголовок должен содержать от 5 до 80 символов")]
    public string? Title { get; init; }

    [Required(ErrorMessage = "Укажите лабораторию")]
    [Range(1, int.MaxValue, ErrorMessage = "laboratoryId должен быть целым числом больше 0")]
    public int? LaboratoryId { get; init; }

    [Required(ErrorMessage = "Укажите тип отхода")]
    [Range(1, int.MaxValue, ErrorMessage = "wasteTypeId должен быть целым числом больше 0")]
    public int? WasteTypeId { get; init; }

    [Required(ErrorMessage = "Укажите описание")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Описание должно содержать от 10 до 500 символов")]
    public string? Description { get; init; }
}
