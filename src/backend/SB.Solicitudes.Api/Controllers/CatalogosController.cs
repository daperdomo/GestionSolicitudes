using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Application.Solicitudes;
using SB.Solicitudes.Api.Authorization;
using SB.Solicitudes.Api.Extensions;
using SB.Solicitudes.Application.Catalogos;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/catalogos")]
public sealed class CatalogosController(ISender sender) : ControllerBase
{
    [HttpGet("areas")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogItem>>> GetAreas(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ObtenerAreasQuery(), cancellationToken));

    [HttpGet("tipos-solicitud")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogItem>>> GetRequestTypes(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ObtenerTiposSolicitudQuery(), cancellationToken));

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpGet("administracion")]
    public async Task<ActionResult<CatalogosAdministracion>> GetAdministration(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ObtenerCatalogosAdministracionQuery(), cancellationToken));

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpPost("areas")]
    public async Task<ActionResult<CatalogAdminItem>> CreateArea(
        CrearCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        Result<CatalogAdminItem> result = await sender.Send(new CrearAreaCommand(request), cancellationToken);
        return result.IsSuccess
            ? Created($"/api/catalogos/areas/{result.Value!.Id}", result.Value)
            : this.ToActionResult(result);
    }

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpPut("areas/{id:int}")]
    public async Task<ActionResult<CatalogAdminItem>> UpdateArea(
        int id,
        ActualizarCatalogoRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ActualizarAreaCommand(id, request), cancellationToken));

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpPost("tipos-solicitud")]
    public async Task<ActionResult<CatalogAdminItem>> CreateRequestType(
        CrearCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        Result<CatalogAdminItem> result = await sender.Send(new CrearTipoSolicitudCommand(request), cancellationToken);
        return result.IsSuccess
            ? Created($"/api/catalogos/tipos-solicitud/{result.Value!.Id}", result.Value)
            : this.ToActionResult(result);
    }

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpPut("tipos-solicitud/{id:int}")]
    public async Task<ActionResult<CatalogAdminItem>> UpdateRequestType(
        int id,
        ActualizarCatalogoRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ActualizarTipoSolicitudCommand(id, request), cancellationToken));
}
