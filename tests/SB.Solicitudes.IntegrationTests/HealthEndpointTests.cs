using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SB.Solicitudes.IntegrationTests;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealthWhenApplicationStartsReturnsHealthyResponse()
    {
        using HttpResponseMessage httpResponse = await client.GetAsync("/health");
        HealthResponse? response = await httpResponse.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.NotNull(response);
        Assert.Equal("Healthy", response.Status);
    }

    private sealed record HealthResponse(string Status);
}
