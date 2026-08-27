------------------------------------------------using SB.Solicitudes.Domain.Entities;
using SB.Solicitudes.Domain.Enums;
using SB.Solicitudes.Domain.Rules;

namespace SB.Solicitudes.UnitTests;

public sealed class SolicitudTests
{
    private static readonly Guid RequesterId = Guid.Parse("30000000-0000-0000-0000-000000000003");
    private static readonly Guid AnalystId = Guid.Parse("20000000-0000-0000-0000-000000000002");

    [Fact]
    public void ChangeStatusWithInvalidTransitionFails()
    {
        Solicitud request = CreateRequest();

        RuleResult result = request.CambiarEstado(
            EstadoSolicitud.Cerrada,
            AnalystId,
            RolUsuario.Analista,
            "Intento inválido",
            DateTimeOffset.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal(EstadoSolicitud.Registrada, request.Estado);
    }

    [Fact]
    public void CloseWithoutResolutionCommentFails()
    {
        Solicitud request = CreateResolvedRequest();

        RuleResult result = request.CambiarEstado(
            EstadoSolicitud.Cerrada,
            AnalystId,
            RolUsuario.Analista,
            null,
            DateTimeOffset.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal("resolution_comment_required", result.Code);
    }

    [Fact]
    public void CloseWithResolutionCommentSucceeds()
    {
        Solicitud request = CreateResolvedRequest();

        RuleResult result = request.CambiarEstado(
            EstadoSolicitud.Cerrada,
            AnalystId,
            RolUsuario.Analista,
            "Acceso entregado y validado.",
            DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(EstadoSolicitud.Cerrada, request.Estado);
    }

    [Fact]
    public void ReopenAsRequesterFails()
    {
        Solicitud request = CreateClosedRequest();

        RuleResult result = request.CambiarEstado(
            EstadoSolicitud.EnAnalisis,
            RequesterId,
            RolUsuario.Solicitante,
            "Necesito otra revisión.",
            DateTimeOffset.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal("reopen_forbidden", result.Code);
    }

    [Fact]
    public void ReopenAsAnalystSucceeds()
    {
        Solicitud request = CreateClosedRequest();

        RuleResult result = request.CambiarEstado(
            EstadoSolicitud.EnAnalisis,
            AnalystId,
            RolUsuario.Analista,
            "Se requiere una corrección.",
            DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(EstadoSolicitud.EnAnalisis, request.Estado);
    }

    [Fact]
    public void WaitForRequesterWithoutPublicCommentFails()
    {
        Solicitud request = CreateRequest();
        Assert.True(request.CambiarEstado(EstadoSolicitud.EnAnalisis, AnalystId, RolUsuario.Analista, null, DateTimeOffset.UtcNow).IsSuccess);

        RuleResult result = request.CambiarEstado(
            EstadoSolicitud.EnEsperaSolicitante,
            AnalystId,
            RolUsuario.Analista,
            null,
            DateTimeOffset.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal("public_comment_required", result.Code);
    }

    [Fact]
    public void WaitForRequesterCanReturnToAnalysis()
    {
        Solicitud request = CreateRequest();
        Assert.True(request.CambiarEstado(EstadoSolicitud.EnAnalisis, AnalystId, RolUsuario.Analista, null, DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(request.CambiarEstado(EstadoSolicitud.EnEsperaSolicitante, AnalystId, RolUsuario.Analista, "Adjunte evidencia.", DateTimeOffset.UtcNow).IsSuccess);

        RuleResult result = request.CambiarEstado(
            EstadoSolicitud.EnAnalisis,
            AnalystId,
            RolUsuario.Analista,
            "Evidencia recibida.",
            DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
    }

    private static Solicitud CreateRequest() => Solicitud.Crear(
        "SOL-2026-TEST",
        "Solicitud de prueba",
        "Descripción suficiente para la prueba.",
        PrioridadSolicitud.Media,
        null,
        RequesterId,
        1,
        1,
        null,
        DateTimeOffset.UtcNow);

    private static Solicitud CreateResolvedRequest()
    {
        Solicitud request = CreateRequest();
        Assert.True(request.CambiarEstado(EstadoSolicitud.EnAnalisis, AnalystId, RolUsuario.Analista, null, DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(request.CambiarEstado(EstadoSolicitud.EnProgreso, AnalystId, RolUsuario.Analista, null, DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(request.CambiarEstado(EstadoSolicitud.Resuelta, AnalystId, RolUsuario.Analista, null, DateTimeOffset.UtcNow).IsSuccess);
        return request;
    }

    private static Solicitud CreateClosedRequest()
    {
        Solicitud request = CreateResolvedRequest();
        Assert.True(request.CambiarEstado(EstadoSolicitud.Cerrada, AnalystId, RolUsuario.Analista, "Resuelta.", DateTimeOffset.UtcNow).IsSuccess);
        return request;
    }
}
