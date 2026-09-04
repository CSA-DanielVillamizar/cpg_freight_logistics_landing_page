using System.Net.Http.Headers;

namespace CPG.Api.IntegrationTests.Support;

/// <summary>Scenario-scoped state shared between step definitions (Reqnroll DI).</summary>
public sealed class ScenarioState
{
    public HttpClient Client { get; } = TestApp.Factory.CreateClient();

    public HttpResponseMessage? LastResponse { get; set; }

    public string? LastBody { get; set; }

    public void Authenticate(string accessToken)
        => Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
}
