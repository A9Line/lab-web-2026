using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Services;

// ЛР10: заявки хранятся в PostgreSQL через EF Core
public sealed class WasteRequestService : IWasteRequestService
{
    private readonly AppDbContext _db;
    private readonly bool _simulateCreateFailure;

    public WasteRequestService(AppDbContext db, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _db = db;

        // учебный сбой для проверки лога, только в Development
        _simulateCreateFailure = environment.IsDevelopment()
            && configuration.GetValue<bool>("FailureSimulation:CreateWasteRequest");
    }

    public async Task<IReadOnlyList<WasteRequest>> GetAllAsync()
    {
        return await _db.WasteRequests
            .AsNoTracking()
            .OrderByDescending(r => r.Id)
            .ToListAsync();
    }

    public async Task<WasteRequest?> GetByIdAsync(int id)
    {
        return await _db.WasteRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<WasteRequestOperationResult> CreateAsync(CreateWasteRequestRequest request, int createdByUserId)
    {
        if (_simulateCreateFailure)
        {
            throw new InvalidOperationException("Учебный сбой ЛР8");
        }

        if (!await _db.Laboratories.AnyAsync(l => l.Id == request.LaboratoryId))
        {
            return WasteRequestOperationResult.Invalid("laboratoryId", $"Лаборатории с id={request.LaboratoryId} нет");
        }

        if (!await _db.WasteTypes.AnyAsync(t => t.Id == request.WasteTypeId))
        {
            return WasteRequestOperationResult.Invalid("wasteTypeId", $"Типа отхода с id={request.WasteTypeId} нет");
        }

        if (!await _db.Users.AnyAsync(u => u.Id == createdByUserId))
        {
            return WasteRequestOperationResult.Invalid("X-User-Id", $"Пользователя с id={createdByUserId} нет");
        }

        var now = DateTimeOffset.UtcNow;
        var item = new WasteRequest
        {
            Number = await NextNumberAsync(now.Year),
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

        _db.WasteRequests.Add(item);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "waste_requests_number_key"
        })
        {
            // два запроса одновременно получили один и тот же номер
            return WasteRequestOperationResult.Conflict($"Номер {item.Number} уже занят, повторите запрос");
        }

        return WasteRequestOperationResult.Success(item);
    }

    public async Task<WasteRequestOperationResult> AssignExecutorAsync(int id, int assigneeUserId)
    {
        var item = await _db.WasteRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (item is null)
        {
            return WasteRequestOperationResult.NotFound();
        }

        if (!await _db.Users.AnyAsync(u => u.Id == assigneeUserId))
        {
            return WasteRequestOperationResult.Invalid("assigneeUserId", $"Пользователя с id={assigneeUserId} нет");
        }

        if (item.Status != WasteRequestStatus.New)
        {
            return WasteRequestOperationResult.Conflict(
                $"Назначить исполнителя можно только новой заявке. Текущий статус: {item.Status}");
        }

        item.AssigneeUserId = assigneeUserId;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return WasteRequestOperationResult.Success(item);
    }

    public async Task<WasteRequestOperationResult> ChangeStatusAsync(int id, WasteRequestStatus newStatus, int callerUserId)
    {
        var item = await _db.WasteRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (item is null)
        {
            return WasteRequestOperationResult.NotFound();
        }

        var error = CheckTransition(item, newStatus, callerUserId);
        if (error is not null)
        {
            return WasteRequestOperationResult.Conflict(error);
        }

        item.Status = newStatus;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return WasteRequestOperationResult.Success(item);
    }

    // номер вида WR-2026-0001, счёт идёт заново каждый год
    private async Task<string> NextNumberAsync(int year)
    {
        var prefix = $"WR-{year}-";
        var last = await _db.WasteRequests
            .Where(r => r.Number.StartsWith(prefix))
            .OrderByDescending(r => r.Number)
            .Select(r => r.Number)
            .FirstOrDefaultAsync();

        var next = last is null ? 1 : int.Parse(last[prefix.Length..]) + 1;
        return $"{prefix}{next:D4}";
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
