using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Api.Authorization;
using SB.Solicitudes.Api.Extensions;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Notifications;

namespace SB.Solicitudes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notificaciones")]
public sealed class NotificacionesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationItem>>> GetLatest(
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(
            new ObtenerNotificacionesQuery(User.GetCurrentUser().Id, limit),
            cancellationToken));

    [HttpGet("no-leidas/count")]
    public async Task<ActionResult<UnreadNotificationCount>> CountUnread(CancellationToken cancellationToken) =>
        Ok(await sender.Send(
            new ObtenerContadorNotificacionesQuery(User.GetCurrentUser().Id),
            cancellationToken));

    [HttpPatch("{id:long}/leida")]
    public async Task<IActionResult> MarkAsRead(long id, CancellationToken cancellationToken)
    {
        Result<bool> result = await sender.Send(
            new MarcarNotificacionLeidaCommand(id, User.GetCurrentUser().Id),
            cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result).Result!;
    }

    [HttpPatch("leidas")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await sender.Send(
            new MarcarTodasNotificacionesLeidasCommand(User.GetCurrentUser().Id),
            cancellationToken);
        return NoContent();
    }
}
