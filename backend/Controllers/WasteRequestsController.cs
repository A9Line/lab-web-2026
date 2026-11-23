using System.ComponentModel.DataAnnotations;
using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/waste-requests")]
public class WasteRequestsController : ControllerBase
{
    private const string UserIdHeader = "X-User-Id";

    private readonly IWasteRequestService _service;

    public WasteRequestsController(IWasteRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WasteRequestResponse>>> GetWasteRequests()
    {
        var items = await _service.GetAllAsync();
        return Ok(items.Select(WasteRequestResponse.From));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WasteRequestResponse>> GetWasteRequest(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(WasteRequestResponse.From(item));
    }

    [HttpPost]
    public async Task<ActionResult<WasteRequestResponse>> CreateWasteRequest(
        [FromHeader(Name = UserIdHeader)]
        [Required(ErrorMessage = "Передайте заголовок X-User-Id")]
        [Range(1, int.MaxValue, ErrorMessage = "X-User-Id должен быть целым числом больше 0")]
        int? userId,
        [FromBody] CreateWasteRequestRequest request)
    {
        var result = await _service.CreateAsync(request, userId!.Value);
        if (result.Outcome != WasteRequestOutcome.Success)
        {
            return ToActionResult(0, result);
        }

        var created = result.Item!;
        return CreatedAtAction(nameof(GetWasteRequest), new { id = created.Id }, WasteRequestResponse.From(created));
    }

    [HttpPatch("{id:int}/assignee")]
    public async Task<ActionResult<WasteRequestResponse>> AssignWasteRequest(
        int id,
        [FromHeader(Name = UserIdHeader)]
        [Required(ErrorMessage = "Передайте заголовок X-User-Id")]
        [Range(1, int.MaxValue, ErrorMessage = "X-User-Id должен быть целым числом больше 0")]
        int? userId,
        [FromBody] AssignWasteRequestRequest request)
    {
        // роль (назначает заведующий) будет проверяться в ЛР11
        return ToActionResult(id, await _service.AssignExecutorAsync(id, request.AssigneeUserId!.Value));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<WasteRequestResponse>> ChangeWasteRequestStatus(
        int id,
        [FromHeader(Name = UserIdHeader)]
        [Required(ErrorMessage = "Передайте заголовок X-User-Id")]
        [Range(1, int.MaxValue, ErrorMessage = "X-User-Id должен быть целым числом больше 0")]
        int? userId,
        [FromBody] ChangeWasteRequestStatusRequest request)
    {
        return ToActionResult(id, await _service.ChangeStatusAsync(id, request.Status!.Value, userId!.Value));
    }

    private ActionResult<WasteRequestResponse> ToActionResult(int id, WasteRequestOperationResult result)
    {
        switch (result.Outcome)
        {
            case WasteRequestOutcome.Success:
                return Ok(WasteRequestResponse.From(result.Item!));
            case WasteRequestOutcome.NotFound:
                return NotFoundProblem(id);
            case WasteRequestOutcome.Invalid:
                ModelState.AddModelError(result.Field!, result.Error!);
                return ValidationProblem(ModelState);
            default:
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Действие противоречит состоянию заявки",
                    detail: result.Error);
        }
    }

    private ObjectResult NotFoundProblem(int id)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Заявка не найдена",
            detail: $"Заявки с id={id} нет");
    }
}
