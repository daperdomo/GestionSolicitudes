using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Application.Solicitudes;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Rules;

namespace SB.Solicitudes.Application.Catalogos;

internal sealed class CatalogoAdministrationService(
    ICatalogRepository catalogs,
    IUnitOfWork unitOfWork) : ICatalogoAdministrationService
{
    public async Task<CatalogosAdministracion> GetAllAsync(CancellationToken cancellationToken) =>
        new(
            await catalogs.GetAllAreasAsync(cancellationToken),
            await catalogs.GetAllTiposSolicitudAsync(cancellationToken));

    public async Task<Result<CatalogAdminItem>> CreateAreaAsync(
        CrearCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        OperationError? error = ValidateName(request.Nombre);
        if (error is not null) return Failure(error);

        string name = request.Nombre.Trim();
        if (await catalogs.AreaNameExistsAsync(name, null, cancellationToken)) return Duplicate("area");

        Area area = new(name);
        await catalogs.AddAreaAsync(area, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CatalogAdminItem>.Success(new CatalogAdminItem(area.Id, area.Nombre, area.Activa));
    }

    public async Task<Result<CatalogAdminItem>> UpdateAreaAsync(
        int id,
        ActualizarCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        OperationError? error = ValidateName(request.Nombre);
        if (error is not null) return Failure(error);

        Area? area = await catalogs.GetAreaForUpdateAsync(id, cancellationToken);
        if (area is null) return NotFound("area");

        string name = request.Nombre.Trim();
        if (await catalogs.AreaNameExistsAsync(name, id, cancellationToken)) return Duplicate("area");

        area.Actualizar(name, request.Activo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CatalogAdminItem>.Success(new CatalogAdminItem(area.Id, area.Nombre, area.Activa));
    }

    public async Task<Result<CatalogAdminItem>> CreateRequestTypeAsync(
        CrearCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        OperationError? error = ValidateName(request.Nombre);
        if (error is not null) return Failure(error);

        string name = request.Nombre.Trim();
        if (await catalogs.TipoSolicitudNameExistsAsync(name, null, cancellationToken)) return Duplicate("tipo de solicitud");

        TipoSolicitud type = new(name);
        await catalogs.AddTipoSolicitudAsync(type, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CatalogAdminItem>.Success(new CatalogAdminItem(type.Id, type.Nombre, type.Activo));
    }

    public async Task<Result<CatalogAdminItem>> UpdateRequestTypeAsync(
        int id,
        ActualizarCatalogoRequest request,
        CancellationToken cancellationToken)
    {
        OperationError? error = ValidateName(request.Nombre);
        if (error is not null) return Failure(error);

        TipoSolicitud? type = await catalogs.GetTipoSolicitudForUpdateAsync(id, cancellationToken);
        if (type is null) return NotFound("tipo de solicitud");

        string name = request.Nombre.Trim();
        if (await catalogs.TipoSolicitudNameExistsAsync(name, id, cancellationToken)) return Duplicate("tipo de solicitud");

        type.Actualizar(name, request.Activo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CatalogAdminItem>.Success(new CatalogAdminItem(type.Id, type.Nombre, type.Activo));
    }

    private static OperationError? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new OperationError(ErrorType.Validation, "catalog_name_required", "El nombre es obligatorio.");
        if (name.Trim().Length > FieldLengths.CatalogoNombre)
            return new OperationError(ErrorType.Validation, "catalog_name_too_long", $"El nombre no puede exceder {FieldLengths.CatalogoNombre} caracteres.");
        return null;
    }

    private static Result<CatalogAdminItem> Failure(OperationError error) =>
        Result<CatalogAdminItem>.Failure(error.Type, error.Code, error.Message);

    private static Result<CatalogAdminItem> Duplicate(string catalog) =>
        Result<CatalogAdminItem>.Failure(ErrorType.Conflict, "duplicate_catalog_name", $"Ya existe un registro con ese nombre en el catálogo de {catalog}.");

    private static Result<CatalogAdminItem> NotFound(string catalog) =>
        Result<CatalogAdminItem>.Failure(ErrorType.NotFound, "catalog_item_not_found", $"El registro del catálogo de {catalog} no existe.");
}
