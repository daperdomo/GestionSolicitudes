namespace SB.Solicitudes.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApiIntegrationFixtureGroup : ICollectionFixture<ApiWebApplicationFactory>
{
    public const string Name = "API integration";
}
