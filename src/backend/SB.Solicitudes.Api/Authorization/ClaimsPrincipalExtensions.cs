using System.Security.Claims;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Enums;

namespace SB.Solicitudes.Api.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static CurrentUser GetCurrentUser(this ClaimsPrincipal principal)
    {
        string? idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        string? roleClaim = principal.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(idClaim, out Guid userId)
            || !Enum.TryParse(roleClaim, ignoreCase: false, out RolUsuario role))
        {
            throw new InvalidOperationException("El token autenticado no contiene una identidad válida.");
        }

        return new CurrentUser(userId, role);
    }
}
