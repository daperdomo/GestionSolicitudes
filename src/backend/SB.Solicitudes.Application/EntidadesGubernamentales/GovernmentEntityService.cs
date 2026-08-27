using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.EntidadesGubernamentales;

internal sealed class GovernmentEntityService(
    IGovernmentEntityRepository repository) : IGovernmentEntityService
{
    public async Task<IReadOnlyCollection<GovernmentEntity>> GetAllAsync(CancellationToken cancellationToken) =>
        await repository.GetAllAsync(cancellationToken);

    public async Task<Result<GovernmentEntity>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        GovernmentEntity? entity = await repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Result<GovernmentEntity>.Success(entity);
    }

    public async Task<Result<GovernmentEntity>> CreateAsync(
        GovernmentEntityRequest request,
        CancellationToken cancellationToken)
    {
        OperationError? error = Validate(request);
        if (error is not null)
        {
            return Result<GovernmentEntity>.Failure(error.Type, error.Code, error.Message);
        }

        return Result<GovernmentEntity>.Success(await repository.AddAsync(Normalize(request), cancellationToken));
    }

    public async Task<Result<GovernmentEntity>> UpdateAsync(
        int id,
        GovernmentEntityRequest request,
        CancellationToken cancellationToken)
    {
        OperationError? error = Validate(request);
        if (error is not null)
        {
            return Result<GovernmentEntity>.Failure(error.Type, error.Code, error.Message);
        }

        GovernmentEntity? entity = await repository.UpdateAsync(id, Normalize(request), cancellationToken);
        return entity is null ? NotFound() : Result<GovernmentEntity>.Success(entity);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        bool deleted = await repository.DeleteAsync(id, cancellationToken);
        return deleted
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(ErrorType.NotFound, "government_entity_not_found", "La entidad gubernamental no existe.");
    }

    private static OperationError? Validate(GovernmentEntityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre)
            || string.IsNullOrWhiteSpace(request.Categoria)
            || string.IsNullOrWhiteSpace(request.PoderEstado)
            || string.IsNullOrWhiteSpace(request.Sector))
        {
            return new OperationError(ErrorType.Validation, "required_fields", "Todos los campos son obligatorios.");
        }

        if (request.Nombre.Trim().Length > 107
            || request.Categoria.Trim().Length > 41
            || request.PoderEstado.Trim().Length > 15
            || request.Sector.Trim().Length > 40)
        {
            return new OperationError(ErrorType.Validation, "maximum_length", "Uno o más campos exceden la longitud definida por el archivo fuente.");
        }

        return null;
    }

    private static GovernmentEntityRequest Normalize(GovernmentEntityRequest request) => new(
        request.Nombre.Trim(),
        request.Categoria.Trim(),
        request.PoderEstado.Trim(),
        request.Sector.Trim());

    private static Result<GovernmentEntity> NotFound() => Result<GovernmentEntity>.Failure(
        ErrorType.NotFound,
        "government_entity_not_found",
        "La entidad gubernamental no existe.");
}
