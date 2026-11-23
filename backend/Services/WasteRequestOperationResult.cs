using backend.Models;

namespace backend.Services;

public enum WasteRequestOutcome
{
    Success,
    NotFound,
    Conflict,
    Invalid
}

public sealed record WasteRequestOperationResult(
    WasteRequestOutcome Outcome,
    WasteRequest? Item,
    string? Error,
    string? Field = null)
{
    public static WasteRequestOperationResult Success(WasteRequest item) => new(WasteRequestOutcome.Success, item, null);

    public static WasteRequestOperationResult NotFound() => new(WasteRequestOutcome.NotFound, null, null);

    public static WasteRequestOperationResult Conflict(string error) => new(WasteRequestOutcome.Conflict, null, error);

    // ссылка на несуществующую лабораторию, тип отхода или пользователя — это 400, а не 500
    public static WasteRequestOperationResult Invalid(string field, string error) =>
        new(WasteRequestOutcome.Invalid, null, error, field);
}
