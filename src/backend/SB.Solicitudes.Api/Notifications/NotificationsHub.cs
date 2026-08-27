using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SB.Solicitudes.Api.Authorization;

namespace SB.Solicitudes.Api.Notifications;

[Authorize]
public sealed class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Guid userId = Context.User!.GetCurrentUser().Id;
        await Groups.AddToGroupAsync(Context.ConnectionId, NotificationHubGroups.ForUser(userId));
        await base.OnConnectedAsync();
    }
}

internal static class NotificationHubGroups
{
    public static string ForUser(Guid userId) => $"notifications:{userId:D}";
}
