using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Api.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        OperationError error = result.Error!;
        ProblemDetails problem = new()
        {
            Status = GetStatus(error.Type),
            Title = GetTitle(error.Type),
            Type = error.Code,
            Detail = error.Message,
        };

        return new ObjectResult(problem) { StatusCode = problem.Status };
    }

    private static int GetStatus(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Error de validación",
        ErrorType.Unauthorized => "No autenticado",
        ErrorType.Forbidden => "Acceso denegado",
        ErrorType.NotFound => "Recurso no encontrado",
        ErrorType.Conflict => "Conflicto de negocio",
        _ => "Error interno",
    };
}
