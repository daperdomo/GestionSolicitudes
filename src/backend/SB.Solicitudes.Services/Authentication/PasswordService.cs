using Microsoft.AspNetCore.Identity;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;

namespace SB.Solicitudes.Services.Authentication;

internal sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<Usuario> passwordHasher = new();

    public string Hash(Usuario usuario, string password) => passwordHasher.HashPassword(usuario, password);

    public bool Verify(Usuario usuario, string passwordHash, string password) =>
        passwordHasher.VerifyHashedPassword(usuario, passwordHash, password)
            != PasswordVerificationResult.Failed;
}
