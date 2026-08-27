using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Api.Authorization;
using SB.Solicitudes.Api.Extensions;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.EntidadesGubernamentales;

namespace SB.Solicitudes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/entidades-gubernamentales")]
public sealed class GovernmentEntitiesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<GovernmentEntity>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ObtenerEntidadesGubernamentalesQuery(), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GovernmentEntity>> GetById(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ObtenerEntidadGubernamentalQuery(id), cancellationToken));

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpPost]
    public async Task<ActionResult<GovernmentEntity>> Create(
        GovernmentEntityRequest request,
        CancellationToken cancellationToken)
    {
        Result<GovernmentEntity> result = await sender.Send(
            new CrearEntidadGubernamentalCommand(request),
            cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GovernmentEntity>> Update(
        int id,
        GovernmentEntityRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new ActualizarEntidadGubernamentalCommand(id, request),
            cancellationToken));

    [Authorize(Policy = PolicyNames.Administration)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        Result<bool> result = await sender.Send(new EliminarEntidadGubernamentalCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result).Result!;
    }
}
