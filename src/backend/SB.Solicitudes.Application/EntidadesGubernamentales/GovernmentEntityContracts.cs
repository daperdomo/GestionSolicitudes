using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Application.EntidadesGubernamentales;

public sealed record GovernmentEntity(
    int Id,
    string Nombre,
    string Categoria,
    string PoderEstado,
    string Sector);

public sealed record GovernmentEntityRequest(
    string Nombre,
    string Categoria,
    string PoderEstado,
    string Sector);

public interface IGovernmentEntityRepository
{
    Task<IReadOnlyCollection<GovernmentEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<GovernmentEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<GovernmentEntity> AddAsync(GovernmentEntityRequest request, CancellationToken cancellationToken);
    Task<GovernmentEntity?> UpdateAsync(int id, GovernmentEntityRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public interface IGovernmentEntityService
{
    Task<IReadOnlyCollection<GovernmentEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<GovernmentEntity>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<GovernmentEntity>> CreateAsync(GovernmentEntityRequest request, CancellationToken cancellationToken);
    Task<Result<GovernmentEntity>> UpdateAsync(int id, GovernmentEntityRequest request, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
}
