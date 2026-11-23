using backend.Dtos;
using backend.Models;

namespace backend.Services;

public interface IWasteRequestService
{
    Task<IReadOnlyList<WasteRequest>> GetAllAsync();

    Task<WasteRequest?> GetByIdAsync(int id);

    Task<WasteRequestOperationResult> CreateAsync(CreateWasteRequestRequest request, int createdByUserId);

    Task<WasteRequestOperationResult> AssignExecutorAsync(int id, int assigneeUserId);

    Task<WasteRequestOperationResult> ChangeStatusAsync(int id, WasteRequestStatus newStatus, int callerUserId);
}
