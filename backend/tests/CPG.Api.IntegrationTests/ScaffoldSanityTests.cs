using System.Reflection;
using FluentAssertions;
using Xunit;

namespace CPG.Api.IntegrationTests;

/// <summary>
/// Phase 1 guardrails: the BDD suite is wired and every SPEC.md user story has a
/// transcribed feature file. Scenario bindings arrive with their slices (US-01..US-04).
/// </summary>
public sealed class ScaffoldSanityTests
{
    [Theory]
    [InlineData("Authentication.feature")]
    [InlineData("RateCalculation.feature")]
    [InlineData("CarrierCompliance.feature")]
    [InlineData("LeadGeneration.feature")]
    public void Every_user_story_has_a_transcribed_feature_file(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(fileName, StringComparison.Ordinal));

        resourceName.Should().NotBeNull($"{fileName} must ship embedded with the BDD suite (Reqnroll)");

        using var stream = assembly.GetManifestResourceStream(resourceName!)!;
        using var reader = new StreamReader(stream);
        reader.ReadToEnd().Should().Contain("Feature:");
    }

    [Fact]
    public void Api_entry_point_is_discoverable_for_WebApplicationFactory()
    {
        typeof(Program).Assembly.GetName().Name.Should().Be("CPG.Api");
    }
}
