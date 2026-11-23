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
    public ActionResult<IEnumerable<WasteRequestResponse>> GetWasteRequests()
    {
        return Ok(_service.GetAll().Select(WasteRequestResponse.From));
    }

    [HttpGet("{id:int}")]
    public ActionResult<WasteRequestResponse> GetWasteRequest(int id)
    {
        var item = _service.GetById(id);
        if (item is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(WasteRequestResponse.From(item));
    }

    [HttpPost]
    public ActionResult<WasteRequestResponse> CreateWasteRequest(
        [FromHeader(Name = UserIdHeader)]
        [Required(ErrorMessage = "Передайте заголовок X-User-Id")]
        [Range(1, int.MaxValue, ErrorMessage = "X-User-Id должен быть целым числом больше 0")]
        int? userId,
        [FromBody] CreateWasteRequestRequest request)
    {
        var created = _service.Create(request, userId!.Value);
        return CreatedAtAction(nameof(GetWasteRequest), new { id = created.Id }, WasteRequestResponse.From(created));
    }

    [HttpPatch("{id:int}/assignee")]
    public ActionResult<WasteRequestResponse> AssignWasteRequest(
        int id,
        [FromHeader(Name = UserIdHeader)]
        [Required(ErrorMessage = "Передайте заголовок X-User-Id")]
        [Range(1, int.MaxValue, ErrorMessage = "X-User-Id должен быть целым числом больше 0")]
        int? userId,
        [FromBody] AssignWasteRequestRequest request)
    {
        // роль (назначает заведующий) будет проверяться в ЛР11
        return ToActionResult(id, _service.AssignExecutor(id, request.AssigneeUserId!.Value));
    }

    [HttpPatch("{id:int}/status")]
    public ActionResult<WasteRequestResponse> ChangeWasteRequestStatus(
        int id,
        [FromHeader(Name = UserIdHeader)]
        [Required(ErrorMessage = "Передайте заголовок X-User-Id")]
        [Range(1, int.MaxValue, ErrorMessage = "X-User-Id должен быть целым числом больше 0")]
        int? userId,
        [FromBody] ChangeWasteRequestStatusRequest request)
    {
        return ToActionResult(id, _service.ChangeStatus(id, request.Status!.Value, userId!.Value));
    }

    private ActionResult<WasteRequestResponse> ToActionResult(int id, WasteRequestOperationResult result)
    {
        return result.Outcome switch
        {
            WasteRequestOutcome.Success => Ok(WasteRequestResponse.From(result.Item!)),
            WasteRequestOutcome.NotFound => NotFoundProblem(id),
            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Действие противоречит состоянию заявки",
                detail: result.Error)
        };
    }

    private ObjectResult NotFoundProblem(int id)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Заявка не найдена",
            detail: $"Заявки с id={id} нет");
    }
}
