using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SB.Solicitudes.Api.Middleware;

public sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private static readonly Action<ILogger, string, Exception?> LogUnexpectedError = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(5001, "UnhandledApiException"),
        "Error no controlado al procesar {RequestPath}");

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        bool isConcurrencyConflict = exception is DbUpdateConcurrencyException;
        int statusCode = isConcurrencyConflict
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status500InternalServerError;

        LogUnexpectedError(logger, httpContext.Request.Path, exception);
        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Type = isConcurrencyConflict ? "concurrency_conflict" : "unexpected_error",
                Title = isConcurrencyConflict ? "Conflicto de concurrencia" : "Error interno del servidor",
                Detail = isConcurrencyConflict
                    ? "El recurso fue modificado por otro usuario. Actualice la información e intente nuevamente."
                    : "Ocurrió un error inesperado al procesar la solicitud.",
            },
            Exception = exception,
        });
    }
}
