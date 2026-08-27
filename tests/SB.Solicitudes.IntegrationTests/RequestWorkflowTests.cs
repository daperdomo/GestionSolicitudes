using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SB.Solicitudes.IntegrationTests;

[Collection(ApiIntegrationFixtureGroup.Name)]
public sealed class RequestWorkflowTests
{
    private readonly HttpClient client;

    public RequestWorkflowTests(ApiWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task CriticalRequestWorkflowWithValidRolesSucceeds()
    {
        LoginResponse requester = await LoginAsync("solicitante@sb.local", "Solicita1234!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requester.AccessToken);

        CatalogItem[] areas = await client.GetFromJsonAsync<CatalogItem[]>("/api/catalogos/areas") ?? [];
        CatalogItem[] requestTypes = await client.GetFromJsonAsync<CatalogItem[]>("/api/catalogos/tipos-solicitud") ?? [];
        Assert.NotEmpty(areas);
        Assert.NotEmpty(requestTypes);

        using HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/solicitudes", new
        {
            titulo = "Solicitud creada desde prueba de integración",
            descripcion = "Valida autenticación, creación, consulta y transición de estado.",
            prioridad = "Alta",
            fechaCompromiso = DateTimeOffset.UtcNow.AddDays(5),
            areaId = areas[0].Id,
            tipoSolicitudId = requestTypes[0].Id,
            evidenciaReferencia = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        RequestDetail? created = await createResponse.Content.ReadFromJsonAsync<RequestDetail>();
        Assert.NotNull(created);

        UnreadCount? initialUnread = await client.GetFromJsonAsync<UnreadCount>("/api/notificaciones/no-leidas/count");
        Assert.NotNull(initialUnread);
        Assert.True(initialUnread.Total > 0);
        NotificationRecord[] notificationItems =
            await client.GetFromJsonAsync<NotificationRecord[]>("/api/notificaciones?limit=20") ?? [];
        NotificationRecord createdNotification = Assert.Single(
            notificationItems,
            notification => notification.SolicitudId == created.Id && !notification.Leida);
        using HttpResponseMessage readResponse = await client.PatchAsync(
            $"/api/notificaciones/{createdNotification.Id}/leida",
            null);
        Assert.Equal(HttpStatusCode.NoContent, readResponse.StatusCode);
        UnreadCount? updatedUnread = await client.GetFromJsonAsync<UnreadCount>("/api/notificaciones/no-leidas/count");
        Assert.NotNull(updatedUnread);
        Assert.Equal(initialUnread.Total - 1, updatedUnread.Total);

        using HttpResponseMessage listResponse = await client.GetAsync("/api/solicitudes?pageNumber=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        string listJson = await listResponse.Content.ReadAsStringAsync();
        Assert.Contains(created.Codigo, listJson, StringComparison.Ordinal);

        LoginResponse analyst = await LoginAsync("analista@sb.local", "Analista1234!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", analyst.AccessToken);
        using HttpResponseMessage statusResponse = await client.PatchAsJsonAsync($"/api/solicitudes/{created.Id}/estado", new
        {
            estado = "EnAnalisis",
            comentario = "Solicitud recibida para análisis.",
            rowVersion = created.RowVersion,
        });
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        RequestDetail? updated = await statusResponse.Content.ReadFromJsonAsync<RequestDetail>();
        Assert.NotNull(updated);
        Assert.Equal("EnAnalisis", updated.Estado);
        Assert.Contains(updated.Historial, item => item.EstadoNuevo == "EnAnalisis");

        using HttpResponseMessage stalePriorityResponse = await client.PatchAsJsonAsync($"/api/solicitudes/{created.Id}/prioridad", new
        {
            prioridad = "Critica",
            rowVersion = created.RowVersion,
        });
        Assert.Equal(HttpStatusCode.Conflict, stalePriorityResponse.StatusCode);

        using HttpResponseMessage assignmentResponse = await client.PatchAsJsonAsync($"/api/solicitudes/{created.Id}/asignacion", new
        {
            responsableId = requester.UsuarioId,
        });
        Assert.Equal(HttpStatusCode.OK, assignmentResponse.StatusCode);
        RequestDetail? assigned = await assignmentResponse.Content.ReadFromJsonAsync<RequestDetail>();
        Assert.NotNull(assigned);
        Assert.Equal(requester.UsuarioId, assigned.ResponsableId);
        Assert.Equal("EnAnalisis", assigned.Estado);
        Assert.Contains(assigned.Actividad, item => item.Tipo == "Asignacion");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requester.AccessToken);
        using HttpResponseMessage forbiddenStatusResponse = await client.PatchAsJsonAsync($"/api/solicitudes/{created.Id}/estado", new
        {
            estado = "EnProgreso",
            comentario = (string?)null,
            rowVersion = assigned.RowVersion,
        });
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenStatusResponse.StatusCode);
    }

    [Fact]
    public async Task GetRequestsWithoutTokenReturnsUnauthorized()
    {
        client.DefaultRequestHeaders.Authorization = null;
        using HttpResponseMessage response = await client.GetAsync("/api/solicitudes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GovernmentEntitiesEndpointReturnsImportedCatalog()
    {
        LoginResponse administrator = await LoginAsync("admin@sb.local", "Admin1234!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administrator.AccessToken);

        GovernmentEntity[] entities = await client.GetFromJsonAsync<GovernmentEntity[]>("/api/entidades-gubernamentales") ?? [];

        Assert.Equal(181, entities.Length);
        Assert.Equal(181, entities.Select(entity => entity.Id).Distinct().Count());
        Assert.All(entities, entity =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entity.Nombre));
            Assert.False(string.IsNullOrWhiteSpace(entity.Categoria));
            Assert.False(string.IsNullOrWhiteSpace(entity.PoderEstado));
            Assert.False(string.IsNullOrWhiteSpace(entity.Sector));
        });
    }

    [Fact]
    public async Task UserRegistrationIsRestrictedAndSupportsDeactivation()
    {
        var newUser = new
        {
            nombre = "Usuario de integración",
            correo = "usuario.integracion@sb.local",
            password = "Temporal123!",
            rol = "Analista",
        };

        LoginResponse requester = await LoginAsync("solicitante@sb.local", "Solicita1234!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requester.AccessToken);
        using HttpResponseMessage forbiddenResponse = await client.PostAsJsonAsync("/api/usuarios", newUser);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        LoginResponse administrator = await LoginAsync("admin@sb.local", "Admin1234!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administrator.AccessToken);
        using HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/usuarios", newUser);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        UserRecord? created = await createResponse.Content.ReadFromJsonAsync<UserRecord>();
        Assert.NotNull(created);
        Assert.True(created.Activo);

        LoginResponse registeredUser = await LoginAsync(newUser.correo, newUser.password);
        Assert.False(string.IsNullOrWhiteSpace(registeredUser.AccessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administrator.AccessToken);
        using HttpResponseMessage updateResponse = await client.PutAsJsonAsync($"/api/usuarios/{created.Id}", new
        {
            newUser.nombre,
            newUser.correo,
            password = (string?)null,
            newUser.rol,
            activo = false,
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        using HttpResponseMessage inactiveLogin = await client.PostAsJsonAsync("/api/auth/login", new
        {
            newUser.correo,
            newUser.password,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, inactiveLogin.StatusCode);
    }

    [Fact]
    public async Task PublicRegistrationAlwaysCreatesRequesterRole()
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            nombre = "Solicitante auto registrado",
            correo = "auto.registro@sb.local",
            password = "Registro123!",
            rol = "Administrador",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        LoginResponse login = await LoginAsync("auto.registro@sb.local", "Registro123!");
        Assert.Equal("Solicitante", login.Rol);
    }

    private async Task<LoginResponse> LoginAsync(string email, string password)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new { correo = email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new JsonException("La respuesta de autenticación no contiene un cuerpo válido.");
    }

    private sealed record LoginResponse(string AccessToken, string Rol, Guid UsuarioId);
    private sealed record CatalogItem(int Id, string Nombre);
    private sealed record GovernmentEntity(int Id, string Nombre, string Categoria, string PoderEstado, string Sector);
    private sealed record UserRecord(Guid Id, bool Activo);
    private sealed record HistoryItem(string EstadoNuevo);
    private sealed record ActivityItem(string Tipo);
    private sealed record UnreadCount(int Total);
    private sealed record NotificationRecord(long Id, long SolicitudId, bool Leida);
    private sealed record RequestDetail(
        long Id,
        string Codigo,
        string Estado,
        string RowVersion,
        Guid? ResponsableId,
        IReadOnlyCollection<HistoryItem> Historial,
        IReadOnlyCollection<ActivityItem> Actividad);
}
