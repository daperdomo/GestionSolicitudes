using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Api.Authorization;
using SB.Solicitudes.Api.Extensions;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Solicitudes;

namespace SB.Solicitudes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/solicitudes")]
public sealed class SolicitudesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<SolicitudListItem>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SolicitudListItem>>> Search(
        [FromQuery] SolicitudFilter filter,
        CancellationToken cancellationToken)
    {
        Result<PagedResult<SolicitudListItem>> result = await sender.Send(
            new BuscarSolicitudesQuery(filter, User.GetCurrentUser()),
            cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType<SolicitudDetail>(StatusCodes.Status201Created)]
    public async Task<ActionResult<SolicitudDetail>> Create(
        CrearSolicitudRequest request,
        CancellationToken cancellationToken)
    {
        Result<SolicitudDetail> result = await sender.Send(
            new CrearSolicitudCommand(request, User.GetCurrentUser()),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType<SolicitudDetail>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SolicitudDetail>> GetById(long id, CancellationToken cancellationToken)
    {
        Result<SolicitudDetail> result = await sender.Send(
            new ObtenerSolicitudQuery(id, User.GetCurrentUser()),
            cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Policy = PolicyNames.ManageRequests)]
    [HttpPatch("{id:long}/estado")]
    public async Task<ActionResult<SolicitudDetail>> ChangeStatus(
        long id,
        CambiarEstadoRequest request,
        CancellationToken cancellationToken)
    {
        Result<SolicitudDetail> result = await sender.Send(
            new CambiarEstadoSolicitudCommand(id, request, User.GetCurrentUser()),
            cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Policy = PolicyNames.ManageRequests)]
    [HttpPatch("{id:long}/asignacion")]
    public async Task<ActionResult<SolicitudDetail>> Assign(
        long id,
        AsignarSolicitudRequest request,
        CancellationToken cancellationToken)
    {
        Result<SolicitudDetail> result = await sender.Send(
            new AsignarSolicitudCommand(id, request, User.GetCurrentUser()),
            cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:long}/comentarios")]
    public async Task<ActionResult<SolicitudDetail>> AddComment(
        long id,
        AgregarComentarioRequest request,
        CancellationToken cancellationToken)
    {
        Result<SolicitudDetail> result = await sender.Send(
            new AgregarComentarioSolicitudCommand(id, request, User.GetCurrentUser()),
            cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Policy = PolicyNames.ManageRequests)]
    [HttpPatch("{id:long}/prioridad")]
    public async Task<ActionResult<SolicitudDetail>> ChangePriority(long id, CambiarPrioridadRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new CambiarPrioridadSolicitudCommand(id, request, User.GetCurrentUser()),
            cancellationToken));

    [Authorize(Policy = PolicyNames.ManageRequests)]
    [HttpPatch("{id:long}/fecha-compromiso")]
    public async Task<ActionResult<SolicitudDetail>> ChangeDueDate(long id, CambiarFechaCompromisoRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new CambiarFechaCompromisoSolicitudCommand(id, request, User.GetCurrentUser()),
            cancellationToken));

    [Authorize(Policy = PolicyNames.ManageRequests)]
    [HttpPatch("{id:long}/area")]
    public async Task<ActionResult<SolicitudDetail>> ChangeArea(long id, CambiarAreaRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new CambiarAreaSolicitudCommand(id, request, User.GetCurrentUser()),
            cancellationToken));

    [Authorize(Policy = PolicyNames.ManageRequests)]
    [HttpPatch("{id:long}/tipo")]
    public async Task<ActionResult<SolicitudDetail>> ChangeRequestType(long id, CambiarTipoSolicitudRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new CambiarTipoSolicitudCommand(id, request, User.GetCurrentUser()),
            cancellationToken));
}
