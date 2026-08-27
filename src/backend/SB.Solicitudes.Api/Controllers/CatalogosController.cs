using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Application.Solicitudes;

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
}
