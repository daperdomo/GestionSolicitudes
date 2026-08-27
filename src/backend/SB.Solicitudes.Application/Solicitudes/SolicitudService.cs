using SB.Solicitudes.Application.Auth;
using SB.Solicitudes.Application.Common;
using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;
using SB.Solicitudes.Domain.Rules;

namespace SB.Solicitudes.Application.Solicitudes;

internal sealed class SolicitudService(
    ISolicitudRepository solicitudes,
    ICatalogRepository catalogs,
    IUsuarioRepository users,
    ISolicitudCodeGenerator codeGenerator,
    INotificationRepository notifications,
    INotificationDispatcher notificationDispatcher,
    IUnitOfWork unitOfWork) : ISolicitudService
{
    public async Task<Result<SolicitudDetail>> CreateAsync(
        CrearSolicitudRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        OperationError? validationError = ValidateCreate(request);
        if (validationError is not null)
        {
            return Result<SolicitudDetail>.Failure(validationError.Type, validationError.Code, validationError.Message);
        }

        if (!await catalogs.AreaExistsAsync(request.AreaId, cancellationToken)
            || !await catalogs.TipoSolicitudExistsAsync(request.TipoSolicitudId, cancellationToken))
        {
            return Result<SolicitudDetail>.Failure(
                ErrorType.Validation,
                "invalid_catalog",
                "El área o tipo de solicitud indicado no existe o está inactivo.");
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        Usuario? responsible = null;
        if (request.ResponsableId.HasValue)
        {
            if (currentUser.Rol == RolUsuario.Solicitante)
            {
                return Forbidden();
            }

            responsible = await users.GetByIdAsync(request.ResponsableId.Value, cancellationToken);
            if (responsible is null || !responsible.Activo)
            {
                return Result<SolicitudDetail>.Failure(ErrorType.Validation, "invalid_assignee", "El responsable debe ser un usuario activo.");
            }
        }

        string code = await codeGenerator.NextAsync(now, cancellationToken);
        Solicitud solicitud = Solicitud.Crear(
            code,
            request.Titulo,
            request.Descripcion,
            request.Prioridad,
            request.FechaCompromiso,
            currentUser.Id,
            request.AreaId,
            request.TipoSolicitudId,
            request.EvidenciaReferencia,
            request.ResponsableId,
            now);

        await solicitudes.AddAsync(solicitud, cancellationToken);
        Notificacion notification = CreateNotification(
            solicitud,
            currentUser.Id,
            "Solicitud registrada",
            $"La solicitud {solicitud.Codigo} fue registrada correctamente.");
        await notifications.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await DispatchAsync(notification, cancellationToken);

        if (responsible is not null && responsible.Id != currentUser.Id)
        {
            Notificacion assignmentNotification = CreateNotification(
                solicitud,
                responsible.Id,
                "Solicitud asignada",
                $"La solicitud {solicitud.Codigo} fue asignada a su gestión.");
            await notifications.AddAsync(assignmentNotification, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await DispatchAsync(assignmentNotification, cancellationToken);
        }

        return await GetDetailResultAsync(solicitud.Id, currentUser, cancellationToken);
    }

    public async Task<Result<PagedResult<SolicitudListItem>>> SearchAsync(
        SolicitudFilter filter,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (filter.PageNumber < 1 || filter.PageSize is < 1 or > 100)
        {
            return Result<PagedResult<SolicitudListItem>>.Failure(
                ErrorType.Validation,
                "invalid_pagination",
                "pageNumber debe ser mayor que cero y pageSize debe estar entre 1 y 100.");
        }

        PagedResult<SolicitudListItem> page = await solicitudes.SearchAsync(filter, currentUser, cancellationToken);
        return Result<PagedResult<SolicitudListItem>>.Success(page);
    }

    public async Task<Result<SolicitudDetail>> GetByIdAsync(
        long id,
        CurrentUser currentUser,
        CancellationToken cancellationToken) =>
        await GetDetailResultAsync(id, currentUser, cancellationToken);

    public async Task<Result<SolicitudDetail>> ChangeStatusAsync(
        long id,
        CambiarEstadoRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Rol == RolUsuario.Solicitante)
        {
            return Forbidden();
        }

        Solicitud? solicitud = await solicitudes.GetForUpdateAsync(id, cancellationToken);
        if (solicitud is null)
        {
            return NotFound();
        }


        if (!TrySetRowVersion(solicitud, request.RowVersion))
        {
            return InvalidRowVersion();
        }

        RuleResult rule = solicitud.CambiarEstado(
            request.Estado,
            currentUser.Id,
            currentUser.Rol,
            request.Comentario,
            DateTimeOffset.UtcNow);

        if (!rule.IsSuccess)
        {
            return Result<SolicitudDetail>.Failure(ErrorType.Conflict, rule.Code, rule.Error);
        }

        Notificacion notification = CreateNotification(
            solicitud,
            solicitud.UsuarioSolicitanteId,
            "Estado de solicitud actualizado",
            $"La solicitud {solicitud.Codigo} cambió al estado {solicitud.Estado}.");
        await notifications.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await DispatchAsync(notification, cancellationToken);
        return await GetDetailResultAsync(id, currentUser, cancellationToken);
    }

    public async Task<Result<SolicitudDetail>> AssignAsync(
        long id,
        AsignarSolicitudRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Rol == RolUsuario.Solicitante)
        {
            return Forbidden();
        }

        Usuario? responsible = request.ResponsableId.HasValue
            ? await users.GetByIdAsync(request.ResponsableId.Value, cancellationToken)
            : null;
        if (request.ResponsableId.HasValue && (responsible is null || !responsible.Activo))
        {
            return Result<SolicitudDetail>.Failure(
                ErrorType.Validation,
                "invalid_assignee",
                "El responsable debe ser un usuario activo.");
        }

        Solicitud? solicitud = await solicitudes.GetForUpdateAsync(id, cancellationToken);
        if (solicitud is null)
        {
            return NotFound();
        }

        RuleResult rule = solicitud.Asignar(
            request.ResponsableId,
            currentUser.Id,
            request.Comentario,
            DateTimeOffset.UtcNow);

        if (!rule.IsSuccess)
        {
            return Result<SolicitudDetail>.Failure(ErrorType.Conflict, rule.Code, rule.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        if (responsible is not null)
        {
            Notificacion notification = CreateNotification(
                solicitud,
                responsible.Id,
                "Solicitud asignada",
                $"La solicitud {solicitud.Codigo} fue asignada a su gestión.");
            await notifications.AddAsync(notification, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await DispatchAsync(notification, cancellationToken);
        }
        return await GetDetailResultAsync(id, currentUser, cancellationToken);
    }

    public async Task<Result<SolicitudDetail>> AddCommentAsync(
        long id,
        AgregarComentarioRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        Solicitud? solicitud = await solicitudes.GetForUpdateAsync(id, cancellationToken);
        if (solicitud is null || !CanAccess(solicitud, currentUser))
        {
            return NotFound();
        }

        if (currentUser.Rol == RolUsuario.Solicitante && request.Visibilidad == VisibilidadComentario.Interno)
        {
            return Forbidden();
        }

        RuleResult rule = solicitud.AgregarComentario(
            currentUser.Id,
            request.Texto,
            request.Visibilidad,
            DateTimeOffset.UtcNow);

        if (!rule.IsSuccess)
        {
            return Result<SolicitudDetail>.Failure(ErrorType.Validation, rule.Code, rule.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetDetailResultAsync(id, currentUser, cancellationToken);
    }

    public async Task<Result<SolicitudDetail>> ChangePriorityAsync(
        long id,
        CambiarPrioridadRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Rol == RolUsuario.Solicitante) return Forbidden();
        if (!Enum.IsDefined(request.Prioridad)) return Result<SolicitudDetail>.Failure(ErrorType.Validation, "invalid_priority", "La prioridad indicada no es válida.");
        Solicitud? solicitud = await solicitudes.GetForUpdateAsync(id, cancellationToken);
        if (solicitud is null) return NotFound();
        if (!TrySetRowVersion(solicitud, request.RowVersion)) return InvalidRowVersion();
        RuleResult rule = solicitud.CambiarPrioridad(request.Prioridad, currentUser.Id, DateTimeOffset.UtcNow);
        return await SaveFieldChangeAsync(id, solicitud, rule, currentUser, cancellationToken);
    }

    public async Task<Result<SolicitudDetail>> ChangeDueDateAsync(
        long id,
        CambiarFechaCompromisoRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Rol == RolUsuario.Solicitante) return Forbidden();
        Solicitud? solicitud = await solicitudes.GetForUpdateAsync(id, cancellationToken);
        if (solicitud is null) return NotFound();
        if (!TrySetRowVersion(solicitud, request.RowVersion)) return InvalidRowVersion();
        RuleResult rule = solicitud.CambiarFechaCompromiso(request.FechaCompromiso, currentUser.Id, DateTimeOffset.UtcNow);
        return await SaveFieldChangeAsync(id, solicitud, rule, currentUser, cancellationToken);
    }

    public async Task<Result<SolicitudDetail>> ChangeAreaAsync(
        long id,
        CambiarAreaRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Rol == RolUsuario.Solicitante) return Forbidden();
        Solicitud? solicitud = await solicitudes.GetForUpdateAsync(id, cancellationToken);
        if (solicitud is null) return NotFound();
        CatalogItem? area = await catalogs.GetAreaByIdAsync(request.AreaId, cancellationToken);
        if (area is null) return Result<SolicitudDetail>.Failure(ErrorType.Validation, "invalid_area", "El área indicada no existe o está inactiva.");
        if (!TrySetRowVersion(solicitud, request.RowVersion)) return InvalidRowVersion();
        RuleResult rule = solicitud.CambiarArea(area.Id, solicitud.Area.Nombre, area.Nombre, currentUser.Id, DateTimeOffset.UtcNow);
        return await SaveFieldChangeAsync(id, solicitud, rule, currentUser, cancellationToken);
    }

    public async Task<Result<SolicitudDetail>> ChangeRequestTypeAsync(
        long id,
        CambiarTipoSolicitudRequest request,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Rol == RolUsuario.Solicitante) return Forbidden();
        Solicitud? solicitud = await solicitudes.GetForUpdateAsync(id, cancellationToken);
        if (solicitud is null) return NotFound();
        CatalogItem? type = await catalogs.GetTipoSolicitudByIdAsync(request.TipoSolicitudId, cancellationToken);
        if (type is null) return Result<SolicitudDetail>.Failure(ErrorType.Validation, "invalid_request_type", "El tipo indicado no existe o está inactivo.");
        if (!TrySetRowVersion(solicitud, request.RowVersion)) return InvalidRowVersion();
        RuleResult rule = solicitud.CambiarTipoSolicitud(type.Id, solicitud.TipoSolicitud.Nombre, type.Nombre, currentUser.Id, DateTimeOffset.UtcNow);
        return await SaveFieldChangeAsync(id, solicitud, rule, currentUser, cancellationToken);
    }

    private static OperationError? ValidateCreate(CrearSolicitudRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo) || string.IsNullOrWhiteSpace(request.Descripcion))
        {
            return new OperationError(ErrorType.Validation, "required_fields", "El título y la descripción son obligatorios.");
        }

        if (request.Titulo.Trim().Length > FieldLengths.SolicitudTitulo
            || request.Descripcion.Trim().Length > FieldLengths.SolicitudDescripcion
            || request.EvidenciaReferencia?.Trim().Length > FieldLengths.EvidenciaReferencia)
        {
            return new OperationError(ErrorType.Validation, "maximum_length", "Uno o más campos exceden la longitud permitida.");
        }

        if (!Enum.IsDefined(request.Prioridad))
        {
            return new OperationError(ErrorType.Validation, "invalid_priority", "La prioridad indicada no es válida.");
        }

        return null;
    }

    private async Task<Result<SolicitudDetail>> GetDetailResultAsync(
        long id,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        bool includeInternal = currentUser.Rol != RolUsuario.Solicitante;
        SolicitudDetail? detail = await solicitudes.GetDetailAsync(id, includeInternal, cancellationToken);

        if (detail is null || (currentUser.Rol == RolUsuario.Solicitante && detail.UsuarioSolicitanteId != currentUser.Id))
        {
            return NotFound();
        }

        return Result<SolicitudDetail>.Success(detail);
    }

    private static bool CanAccess(Solicitud solicitud, CurrentUser currentUser) =>
        currentUser.Rol != RolUsuario.Solicitante || solicitud.UsuarioSolicitanteId == currentUser.Id;

    private static Notificacion CreateNotification(
        Solicitud solicitud,
        Guid recipientId,
        string subject,
        string message) => new(
            solicitud,
            recipientId,
            subject,
            message,
            CanalNotificacion.Sistema,
            DateTimeOffset.UtcNow);

    private async Task DispatchAsync(Notificacion notification, CancellationToken cancellationToken)
    {
        await notificationDispatcher.DispatchAsync(notification, cancellationToken);
        notification.MarcarComoEnviada(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private bool TrySetRowVersion(Solicitud solicitud, string rowVersion)
    {
        try
        {
            byte[] value = Convert.FromBase64String(rowVersion);
            if (value.Length == 0) return false;
            solicitudes.SetOriginalRowVersion(solicitud, value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private async Task<Result<SolicitudDetail>> SaveFieldChangeAsync(
        long id,
        Solicitud solicitud,
        RuleResult rule,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (!rule.IsSuccess)
        {
            return Result<SolicitudDetail>.Failure(ErrorType.Conflict, rule.Code, rule.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetDetailResultAsync(id, currentUser, cancellationToken);
    }

    private static Result<SolicitudDetail> InvalidRowVersion() => Result<SolicitudDetail>.Failure(
        ErrorType.Validation,
        "invalid_row_version",
        "La versión de la solicitud no es válida. Actualice la pantalla e intente nuevamente.");

    private static Result<SolicitudDetail> NotFound() => Result<SolicitudDetail>.Failure(
        ErrorType.NotFound,
        "request_not_found",
        "La solicitud no existe o no está disponible para el usuario actual.");

    private static Result<SolicitudDetail> Forbidden() => Result<SolicitudDetail>.Failure(
        ErrorType.Forbidden,
        "operation_forbidden",
        "El usuario actual no tiene permiso para realizar esta operación.");
}
