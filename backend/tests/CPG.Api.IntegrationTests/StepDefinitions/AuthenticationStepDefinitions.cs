using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CPG.Api.IntegrationTests.Support;
using CPG.Application.Features.Authentication;
using CPG.Infrastructure.Persistence;
using FluentAssertions;
using Reqnroll;

namespace CPG.Api.IntegrationTests.StepDefinitions;

[Binding]
public sealed class AuthenticationStepDefinitions(ScenarioState state)
{
    private static readonly IReadOnlyDictionary<string, string> RoleToEmail = new Dictionary<string, string>
    {
        ["Admin"] = "admin@cpgorlando.com",
        ["Carrier"] = "carrier@cpgorlando.com",
        ["Shipper"] = "shipper@cpgorlando.com",
    };

    private string _email = string.Empty;

    [Given(@"a user exists with email ""(.*)"" and role ""(.*)""")]
    public async Task GivenAUserExistsWithEmailAndRole(string email, string role)
    {
        _email = email;

        // The RBAC baseline users are seeded on host start; prove the login path works.
        var response = await state.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = ApplicationDbContextInitialiser.SeedPassword,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK, "the {0} user must be seeded", role);
    }

    [Given(@"an authenticated user with role ""(.*)""")]
    public async Task GivenAnAuthenticatedUserWithRole(string role)
    {
        var email = RoleToEmail[role];
        var login = await state.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = ApplicationDbContextInitialiser.SeedPassword,
        });
        login.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var accessToken = document.RootElement.GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();
        state.Authenticate(accessToken!);
    }

    [When(@"the user sends a POST request to ""(.*)"" with valid credentials")]
    public async Task WhenTheUserSendsAPostRequestWithValidCredentials(string path)
    {
        state.LastResponse = await state.Client.PostAsJsonAsync(path, new LoginRequest
        {
            Email = _email,
            Password = ApplicationDbContextInitialiser.SeedPassword,
        });
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
    }

    [When(@"the user attempts to send a GET request to ""(.*)""")]
    public async Task WhenTheUserAttemptsToSendAGetRequestTo(string path)
    {
        state.LastResponse = await state.Client.GetAsync(path);
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
    }

    [Then(@"the response status code should be (\d+)")]
    public void ThenTheResponseStatusCodeShouldBe(int statusCode)
    {
        state.LastResponse.Should().NotBeNull();
        ((int)state.LastResponse!.StatusCode).Should().Be(statusCode);
    }

    [Then(@"the response status code should be (\d+) Forbidden")]
    public void ThenTheResponseStatusCodeShouldBeForbidden(int statusCode)
        => ThenTheResponseStatusCodeShouldBe(statusCode);

    [Then(@"the response body should contain a valid JWT access token and a refresh token")]
    public void ThenTheResponseBodyShouldContainAValidJwtAndRefreshToken()
    {
        state.LastBody.Should().NotBeNullOrWhiteSpace();

        using var document = JsonDocument.Parse(state.LastBody!);
        var root = document.RootElement;

        var accessToken = root.GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();
        accessToken!.Split('.').Should().HaveCount(3, "a JWT has header.payload.signature");

        root.GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Then(@"the response body (?:must|should) contain an error message ""(.*)""")]
    public void ThenTheResponseBodyMustContainAnErrorMessage(string message)
    {
        state.LastBody.Should().NotBeNull();
        state.LastBody!.Should().Contain(message);
    }
}
