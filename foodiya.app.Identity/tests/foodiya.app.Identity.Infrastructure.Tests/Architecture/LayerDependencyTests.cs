using System.Reflection;

namespace foodiya.app.Identity.Infrastructure.Tests.Architecture;

[TestClass]
public sealed class LayerDependencyTests
{
    [TestMethod]
    public void Assemblies_ReferenceOnlyAllowedIdentityLayers()
    {
        AssertIdentityReferences(
            typeof(Domain.AssemblyReference).Assembly);
        AssertIdentityReferences(
            typeof(Contracts.IdentityServiceMetadata).Assembly);
        AssertIdentityReferences(
            typeof(Application.AssemblyReference).Assembly,
            "foodiya.app.Identity.Contracts",
            "foodiya.app.Identity.Domain");
        AssertIdentityReferences(
            typeof(Infrastructure.AssemblyReference).Assembly,
            "foodiya.app.Identity.Application",
            "foodiya.app.Identity.Contracts",
            "foodiya.app.Identity.Domain",
            "foodiya.app.Messaging");
        AssertIdentityReferences(
            typeof(Program).Assembly,
            "foodiya.app.Identity.Application",
            "foodiya.app.Identity.Contracts",
            "foodiya.app.Identity.Infrastructure");
    }

    private static void AssertIdentityReferences(Assembly assembly, params string[] expected)
    {
        var actual = assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name?.StartsWith("foodiya.", StringComparison.Ordinal) == true
                || name?.StartsWith("Foodiya.", StringComparison.Ordinal) == true)
            .Cast<string>()
            .Order(StringComparer.Ordinal)
            .ToArray();
        var orderedExpected = expected.Order(StringComparer.Ordinal).ToArray();

        Assert.HasCount(orderedExpected.Length, actual,
            $"Unexpected identity-layer dependency in {assembly.GetName().Name}.");
        foreach (var reference in orderedExpected)
        {
            Assert.Contains(reference, actual,
                $"{assembly.GetName().Name} must reference {reference}.");
        }
    }
}
