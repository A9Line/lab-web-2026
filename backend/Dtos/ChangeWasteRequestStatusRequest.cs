using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.Dtos;

public sealed record ChangeWasteRequestStatusRequest
{
    [Required(ErrorMessage = "Укажите статус: New, InProgress, Closed или Cancelled")]
    public WasteRequestStatus? Status { get; init; }
}
