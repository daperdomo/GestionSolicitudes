using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Api.Authorization;
using SB.Solicitudes.Application.Auth;
using SB.Solicitudes.Api.Extensions;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Usuarios;

namespace SB.Solicitudes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/usuarios")]
public sealed class UsuariosController(ISender sender) : ControllerBase
{
    [Authorize(Policy = PolicyNames.ManageRequests)]
    [HttpGet("analistas")]
    public async Task<ActionResult<IReadOnlyCollection<UserOption>>> GetAnalysts(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ObtenerAnalistasActivosQuery(), cancellationToken));

    [Authorize(Policy = PolicyNames.ManageRequests)]
    [HttpGet("asignables")]
    public async Task<ActionResult<IReadOnlyCollection<UserOption>>> GetAssignableUsers(
        [FromQuery] string? search,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new BuscarUsuariosAsignablesQuery(search), cancellationToken));

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UsuarioListItem>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ObtenerUsuariosQuery(), cancellationToken));

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioListItem>> GetById(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ObtenerUsuarioQuery(id), cancellationToken));

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpPost]
    public async Task<ActionResult<UsuarioListItem>> Create(
        CrearUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        Result<UsuarioListItem> result = await sender.Send(new CrearUsuarioCommand(request), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UsuarioListItem>> Update(
        Guid id,
        ActualizarUsuarioRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new ActualizarUsuarioCommand(id, request, User.GetCurrentUser()),
            cancellationToken));
}
