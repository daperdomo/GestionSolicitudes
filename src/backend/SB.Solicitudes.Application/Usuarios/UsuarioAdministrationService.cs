using System.Net.Mail;
using SB.Solicitudes.Application.Auth;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;
using SB.Solicitudes.Domain.Rules;

namespace SB.Solicitudes.Application.Usuarios;

internal sealed class UsuarioAdministrationService(
    IUsuarioRepository users,
    IPasswordService passwords,
    IUnitOfWork unitOfWork) : IUsuarioAdministrationService
{
    public Task<IReadOnlyCollection<UsuarioListItem>> GetAllAsync(CancellationToken cancellationToken) =>
        users.GetAllAsync(cancellationToken);

    public async Task<Result<UsuarioListItem>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Usuario? user = await users.GetByIdAsync(id, cancellationToken);
        return user is null
            ? NotFound()
            : Result<UsuarioListItem>.Success(ToListItem(user));
    }

    public async Task<Result<UsuarioListItem>> CreateAsync(
        CrearUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        OperationError? error = Validate(request.Nombre, request.Correo, request.Password, request.Rol, true);
        if (error is not null)
        {
            return Result<UsuarioListItem>.Failure(error.Type, error.Code, error.Message);
        }

        string normalizedEmail = request.Correo.Trim().ToLowerInvariant();
        if (await users.EmailExistsAsync(normalizedEmail, null, cancellationToken))
        {
            return DuplicateEmail();
        }

        Usuario user = new(Guid.NewGuid(), request.Nombre, normalizedEmail, request.Rol);
        user.EstablecerPasswordHash(passwords.Hash(user, request.Password));
        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UsuarioListItem>.Success(ToListItem(user));
    }

    public async Task<Result<UsuarioListItem>> UpdateAsync(
        Guid id,
        ActualizarUsuarioRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        OperationError? error = Validate(request.Nombre, request.Correo, request.Password, request.Rol, false);
        if (error is not null)
        {
            return Result<UsuarioListItem>.Failure(error.Type, error.Code, error.Message);
        }

        Usuario? user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (id == currentUser.Id && (!request.Activo || request.Rol != RolUsuario.Administrador))
        {
            return Result<UsuarioListItem>.Failure(
                ErrorType.Conflict,
                "administrator_self_lockout",
                "Un Administrador no puede desactivar su propia cuenta ni retirar su propio rol.");
        }

        string normalizedEmail = request.Correo.Trim().ToLowerInvariant();
        if (await users.EmailExistsAsync(normalizedEmail, id, cancellationToken))
        {
            return DuplicateEmail();
        }

        user.Actualizar(request.Nombre, normalizedEmail, request.Rol, request.Activo);
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.EstablecerPasswordHash(passwords.Hash(user, request.Password));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<UsuarioListItem>.Success(ToListItem(user));
    }

    private static OperationError? Validate(
        string name,
        string email,
        string? password,
        RolUsuario role,
        bool passwordRequired)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            return new OperationError(ErrorType.Validation, "user_required_fields", "El nombre y el correo son obligatorios.");
        }

        if (name.Trim().Length > FieldLengths.UsuarioNombre || email.Trim().Length > FieldLengths.UsuarioCorreo)
        {
            return new OperationError(ErrorType.Validation, "user_maximum_length", "El nombre o el correo excede la longitud permitida.");
        }

        if (!MailAddress.TryCreate(email.Trim(), out MailAddress? address)
            || !string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return new OperationError(ErrorType.Validation, "invalid_email", "El correo electrónico no tiene un formato válido.");
        }

        if (!Enum.IsDefined(role))
        {
            return new OperationError(ErrorType.Validation, "invalid_role", "El rol indicado no es válido.");
        }

        if (passwordRequired && string.IsNullOrWhiteSpace(password))
        {
            return new OperationError(ErrorType.Validation, "password_required", "La contraseña inicial es obligatoria.");
        }

        if (!string.IsNullOrWhiteSpace(password)
            && (password.Length < 10
                || !password.Any(char.IsUpper)
                || !password.Any(char.IsLower)
                || !password.Any(char.IsDigit)
                || password.All(char.IsLetterOrDigit)))
        {
            return new OperationError(
                ErrorType.Validation,
                "weak_password",
                "La contraseña debe tener al menos 10 caracteres, mayúscula, minúscula, número y símbolo.");
        }

        return null;
    }

    private static UsuarioListItem ToListItem(Usuario user) => new(
        user.Id,
        user.Nombre,
        user.Correo,
        user.Rol,
        user.Activo,
        user.FechaCreacion);

    private static Result<UsuarioListItem> NotFound() => Result<UsuarioListItem>.Failure(
        ErrorType.NotFound,
        "user_not_found",
        "El usuario no existe.");

    private static Result<UsuarioListItem> DuplicateEmail() => Result<UsuarioListItem>.Failure(
        ErrorType.Conflict,
        "duplicate_email",
        "Ya existe un usuario registrado con ese correo.");
}
