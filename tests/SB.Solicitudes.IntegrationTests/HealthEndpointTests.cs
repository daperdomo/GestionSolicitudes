using System.Net;
using System.Net.Http.Json;

namespace SB.Solicitudes.IntegrationTests;

[Collection(ApiIntegrationFixtureGroup.Name)]
public sealed class HealthEndpointTests
{
    private readonly HttpClient client;

    public HealthEndpointTests(ApiWebApplicationFactory factory)
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
