using backend.Models;

namespace backend.Services;

public enum WasteRequestOutcome
{
    Success,
    NotFound,
    Conflict
}

public sealed record WasteRequestOperationResult(WasteRequestOutcome Outcome, WasteRequest? Item, string? Error)
{
    public static WasteRequestOperationResult Success(WasteRequest item) => new(WasteRequestOutcome.Success, item, null);

    public static WasteRequestOperationResult NotFound() => new(WasteRequestOutcome.NotFound, null, null);

    public static WasteRequestOperationResult Conflict(string error) => new(WasteRequestOutcome.Conflict, null, error);
}
