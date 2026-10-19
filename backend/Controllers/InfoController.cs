using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("info")]
public class InfoController : ControllerBase
{
    private readonly IApplicationInfoService _service;

    public InfoController(IApplicationInfoService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_service.GetInfo());
    }
}
