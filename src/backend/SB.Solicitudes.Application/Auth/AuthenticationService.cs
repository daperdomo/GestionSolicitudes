using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.Auth;

internal sealed class AuthenticationService(
    IUsuarioRepository users,
    IPasswordService passwords,
    ITokenService tokens) : IAuthenticationService
{
    public async Task<Result<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<LoginResponse>.Failure(
                ErrorType.Validation,
                "credentials_required",
                "El correo y la contraseña son obligatorios.");
        }

        Domain.Entities.Usuario? user = await users.GetByEmailAsync(
            request.Correo.Trim().ToLowerInvariant(),
            cancellationToken);

        if (user is null || !user.Activo || !passwords.Verify(user, user.PasswordHash, request.Password))
        {
            return Result<LoginResponse>.Failure(
                ErrorType.Unauthorized,
                "invalid_credentials",
                "Las credenciales no son válidas.");
        }

        TokenResult token = tokens.Create(user);
        LoginResponse response = new(
            token.AccessToken,
            token.ExpiresAt,
            user.Id,
            user.Nombre,
            user.Correo,
            user.Rol.ToString());

        return Result<LoginResponse>.Success(response);
    }
}
