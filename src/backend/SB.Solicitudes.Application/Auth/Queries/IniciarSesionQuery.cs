using MediatR;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Auth;

public sealed record IniciarSesionQuery(LoginRequest Request) : IRequest<Result<LoginResponse>>;

internal sealed class IniciarSesionQueryHandler(IAuthenticationService service)
    : IRequestHandler<IniciarSesionQuery, Result<LoginResponse>>
{
    public Task<Result<LoginResponse>> Handle(IniciarSesionQuery query, CancellationToken cancellationToken) =>
        service.LoginAsync(query.Request, cancellationToken);
}
