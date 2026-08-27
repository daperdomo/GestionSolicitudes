using System.Text.Json;
using SB.Solicitudes.Application.EntidadesGubernamentales;

namespace SB.Solicitudes.Infrastructure.GovernmentEntities;

internal sealed class TextFileGovernmentEntityRepository(
    GovernmentEntityFileOptions options) : IGovernmentEntityRepository, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath = options.FilePath;

    public async Task<IReadOnlyCollection<GovernmentEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            return (await ReadAsync(cancellationToken)).OrderBy(entity => entity.Nombre).ToList();
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<GovernmentEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            return (await ReadAsync(cancellationToken)).SingleOrDefault(entity => entity.Id == id);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<GovernmentEntity> AddAsync(
        GovernmentEntityRequest request,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            List<GovernmentEntity> entities = await ReadAsync(cancellationToken);
            int id = entities.Count == 0 ? 1 : entities.Max(entity => entity.Id) + 1;
            GovernmentEntity entity = ToEntity(id, request);
            entities.Add(entity);
            await WriteAsync(entities, cancellationToken);
            return entity;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<GovernmentEntity?> UpdateAsync(
        int id,
        GovernmentEntityRequest request,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            List<GovernmentEntity> entities = await ReadAsync(cancellationToken);
            int index = entities.FindIndex(entity => entity.Id == id);
            if (index < 0)
            {
                return null;
            }

            GovernmentEntity entity = ToEntity(id, request);
            entities[index] = entity;
            await WriteAsync(entities, cancellationToken);
            return entity;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            List<GovernmentEntity> entities = await ReadAsync(cancellationToken);
            int removed = entities.RemoveAll(entity => entity.Id == id);
            if (removed == 0)
            {
                return false;
            }

            await WriteAsync(entities, cancellationToken);
            return true;
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose() => gate.Dispose();

    private async Task<List<GovernmentEntity>> ReadAsync(CancellationToken cancellationToken)
    {
        await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await JsonSerializer.DeserializeAsync<List<GovernmentEntity>>(stream, JsonOptions, cancellationToken) ?? [];
    }

    private async Task WriteAsync(List<GovernmentEntity> entities, CancellationToken cancellationToken)
    {
        string temporaryPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (FileStream stream = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(stream, entities, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static GovernmentEntity ToEntity(int id, GovernmentEntityRequest request) => new(
        id,
        request.Nombre,
        request.Categoria,
        request.PoderEstado,
        request.Sector);
}
