extern alias FoodiyaApi;

using System.Reflection;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Models;
using Foodiya.Infrastructure.Identity;
using foodiya.app.Messaging;

namespace foodiya.app.Identity.Integration.Tests.Architecture;

[TestClass]
public sealed class ServiceBoundaryTests
{
    [TestMethod]
    public void Services_ReferenceOnlyTheirAllowedProjectLayers()
    {
        AssertReferences(typeof(IntegrationEventEnvelope).Assembly);
        AssertReferences(typeof(AppUser).Assembly);
        AssertReferences(typeof(IUnitService).Assembly, "Foodiya.Domain");
        AssertReferences(typeof(IdentityProfileProvisioner).Assembly,
            "Foodiya.Domain", "Foodiya.Application", "foodiya.app.Messaging");
        AssertReferences(typeof(FoodiyaApi::Program).Assembly,
            "Foodiya.Domain", "Foodiya.Application", "Foodiya.Infrastructure", "foodiya.app.Messaging");
    }

    private static void AssertReferences(Assembly assembly, params string[] allowed)
    {
        var references = assembly.GetReferencedAssemblies().Select(reference => reference.Name!)
            .Where(name => name.StartsWith("Foodiya.", StringComparison.Ordinal)
                || name.StartsWith("foodiya.", StringComparison.Ordinal));
        Assert.AreSequenceEqual(allowed.Order(StringComparer.Ordinal), references.Order(StringComparer.Ordinal),
            message: $"Unexpected project dependency in {assembly.GetName().Name}.");
    }
}
