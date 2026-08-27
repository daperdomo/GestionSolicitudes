using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Api.Authorization;
using SB.Solicitudes.Application.Dashboard;

namespace SB.Solicitudes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet("resumen")]
    [ProducesResponseType<DashboardSummary>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummary>> GetSummary(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ObtenerResumenDashboardQuery(User.GetCurrentUser()), cancellationToken));
}
