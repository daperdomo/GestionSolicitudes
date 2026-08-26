using System.Reflection;

namespace SB.Solicitudes.UnitTests;

public sealed class DependencyRuleTests
{
    private static readonly string[] OUTER_LAYER_ASSEMBLIES =
    [
        "SB.Solicitudes.Api",
        "SB.Solicitudes.Application",
        "SB.Solicitudes.Infrastructure",
        "SB.Solicitudes.Services",
    ];

    [Fact]
    public void DomainDoesNotReferenceOuterLayers()
    {
        Assembly domainAssembly = Assembly.Load("SB.Solicitudes.Domain");
        string[] references = domainAssembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(references, OUTER_LAYER_ASSEMBLIES.Contains);
    }

    [Fact]
    public void ApplicationDoesNotReferenceAdaptersOrPresentation()
    {
        Assembly applicationAssembly = Assembly.Load("SB.Solicitudes.Application");
        string[] forbiddenReferences =
        [
            "SB.Solicitudes.Api",
            "SB.Solicitudes.Infrastructure",
            "SB.Solicitudes.Services",
        ];
        string[] references = applicationAssembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(references, forbiddenReferences.Contains);
    }
}
