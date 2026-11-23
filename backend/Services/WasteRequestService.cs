using backend.Dtos;
using backend.Models;

namespace backend.Services;

// ЛР8: заявки хранятся в памяти, после перезапуска список пустой
public sealed class WasteRequestService : IWasteRequestService
{
    private readonly object _sync = new();
    private readonly List<WasteRequest> _items = new();
    private readonly bool _simulateCreateFailure;
    private int _lastId;

    public WasteRequestService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        // учебный сбой для проверки лога, только в Development
        _simulateCreateFailure = environment.IsDevelopment()
            && configuration.GetValue<bool>("FailureSimulation:CreateWasteRequest");
    }

    public IReadOnlyList<WasteRequest> GetAll()
    {
        lock (_sync)
        {
            return _items.OrderByDescending(item => item.Id).ToList();
        }
    }

    public WasteRequest? GetById(int id)
    {
        lock (_sync)
        {
            return _items.FirstOrDefault(item => item.Id == id);
        }
    }

    public WasteRequest Create(CreateWasteRequestRequest request, int createdByUserId)
    {
        if (_simulateCreateFailure)
        {
            throw new InvalidOperationException("Учебный сбой ЛР8");
        }

        lock (_sync)
        {
            var id = ++_lastId;
            var now = DateTimeOffset.UtcNow;
            var item = new WasteRequest
            {
                Id = id,
                Number = $"WR-{now.Year}-{id:D4}",
                Title = request.Title!,
                Description = request.Description!,
                Status = WasteRequestStatus.New,
                LaboratoryId = request.LaboratoryId!.Value,
                WasteTypeId = request.WasteTypeId!.Value,
                CreatedByUserId = createdByUserId,
                AssigneeUserId = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            _items.Add(item);
            return item;
        }
    }

    public WasteRequestOperationResult AssignExecutor(int id, int assigneeUserId)
    {
        lock (_sync)
        {
            var index = _items.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                return WasteRequestOperationResult.NotFound();
            }

            var current = _items[index];
            if (current.Status != WasteRequestStatus.New)
            {
                return WasteRequestOperationResult.Conflict(
                    $"Назначить исполнителя можно только новой заявке. Текущий статус: {current.Status}");
            }

            var updated = current with { AssigneeUserId = assigneeUserId, UpdatedAt = DateTimeOffset.UtcNow };
            _items[index] = updated;
            return WasteRequestOperationResult.Success(updated);
        }
    }

    public WasteRequestOperationResult ChangeStatus(int id, WasteRequestStatus newStatus, int callerUserId)
    {
        lock (_sync)
        {
            var index = _items.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                return WasteRequestOperationResult.NotFound();
            }

            var current = _items[index];
            var error = CheckTransition(current, newStatus, callerUserId);
            if (error is not null)
            {
                return WasteRequestOperationResult.Conflict(error);
            }

            var updated = current with { Status = newStatus, UpdatedAt = DateTimeOffset.UtcNow };
            _items[index] = updated;
            return WasteRequestOperationResult.Success(updated);
        }
    }

    // переходы как в docs/data-model.md; роль заведующего для отмены проверим в ЛР11
    private static string? CheckTransition(WasteRequest current, WasteRequestStatus newStatus, int callerUserId)
    {
        var from = current.Status;

        if (from == WasteRequestStatus.New && newStatus == WasteRequestStatus.InProgress
            || from == WasteRequestStatus.InProgress && newStatus == WasteRequestStatus.Closed)
        {
            if (current.AssigneeUserId is null)
            {
                return "Нельзя взять заявку в работу: исполнитель не назначен";
            }

            if (current.AssigneeUserId != callerUserId)
            {
                return "Менять статус может только назначенный исполнитель заявки";
            }

            return null;
        }

        if (from == WasteRequestStatus.New && newStatus == WasteRequestStatus.Cancelled)
        {
            return null;
        }

        return $"Недопустимый переход статуса: {from} → {newStatus}";
    }
}
