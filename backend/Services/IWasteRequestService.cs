using backend.Dtos;
using backend.Models;

namespace backend.Services;

public interface IWasteRequestService
{
    IReadOnlyList<WasteRequest> GetAll();

    WasteRequest? GetById(int id);

    WasteRequest Create(CreateWasteRequestRequest request, int createdByUserId);

    WasteRequestOperationResult AssignExecutor(int id, int assigneeUserId);

    WasteRequestOperationResult ChangeStatus(int id, WasteRequestStatus newStatus, int callerUserId);
}
