using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Api.Extensions;
using SB.Solicitudes.Application.Auth;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Usuarios;

namespace SB.Solicitudes.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        Result<LoginResponse> result = await sender.Send(new IniciarSesionQuery(request), cancellationToken);
        return this.ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<UsuarioRegistradoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioRegistradoResponse>> Register(
        RegistrarSolicitanteRequest request,
        CancellationToken cancellationToken)
    {
        Result<UsuarioRegistradoResponse> result = await sender.Send(
            new RegistrarSolicitanteCommand(request),
            cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
