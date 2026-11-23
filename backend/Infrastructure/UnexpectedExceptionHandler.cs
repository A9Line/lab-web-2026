using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace backend.Infrastructure;

// Неожиданная ошибка: в лог одна строка, клиенту 500 без стека
public sealed class UnexpectedExceptionHandler : IExceptionHandler
{
    private readonly ILogger<UnexpectedExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public UnexpectedExceptionHandler(
        ILogger<UnexpectedExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // endpoint к этому моменту уже сброшен, поэтому берём его из IExceptionHandlerFeature
        var feature = httpContext.Features.Get<IExceptionHandlerFeature>();
        var endpoint = feature?.Endpoint;
        var operation = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>()?.ActionName
            ?? endpoint?.DisplayName
            ?? "UnknownOperation";
        var wasteRequestId = feature?.RouteValues?["id"]?.ToString() ?? "-";

        _logger.LogError(
            "{Operation} failed; wasteRequestId={WasteRequestId}; error={ErrorType}",
            operation,
            wasteRequestId,
            exception.GetType().Name);
        _logger.LogDebug(exception, "Подробности сбоя {Operation}", operation);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Не удалось обработать запрос",
                Detail = "Внутренняя ошибка сервера. Повторите попытку позже."
            }
        });
    }
}
