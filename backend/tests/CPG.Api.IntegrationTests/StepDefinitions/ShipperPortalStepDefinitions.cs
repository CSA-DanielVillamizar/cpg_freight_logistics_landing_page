using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CPG.Api.IntegrationTests.Support;
using CPG.Application.Features.Authentication;
using CPG.Infrastructure.Persistence;
using FluentAssertions;
using Reqnroll;

namespace CPG.Api.IntegrationTests.StepDefinitions;

[Binding]
public sealed class ShipperPortalStepDefinitions(ScenarioState state)
{
    private Guid _deliveredLoadId;

    [Given(@"a shipper is authenticated")]
    public async Task GivenAShipperIsAuthenticated()
        => await AuthenticateAsync("shipper@cpgorlando.com");

    [Given(@"a carrier is authenticated for the shipper portal")]
    public async Task GivenACarrierIsAuthenticated()
        => await AuthenticateAsync("carrier@cpgorlando.com");

    [When(@"the shipper requests their loads")]
    [When(@"the carrier requests the shipper loads")]
    public async Task WhenTheUserRequestsShipperLoads()
    {
        state.LastResponse = await state.Client.GetAsync("/api/shipper/loads");
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
    }

    [Then(@"the active shipments include an InTransit or Dispatched load")]
    public void ThenActiveShipmentsIncludeALiveLoad()
    {
        state.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);
        using var document = JsonDocument.Parse(state.LastBody!);
        var active = document.RootElement.GetProperty("active").EnumerateArray().ToList();

        active.Should().NotBeEmpty();
        active.Should().OnlyContain(load =>
            load.GetProperty("status").GetString() == "InTransit"
            || load.GetProperty("status").GetString() == "Dispatched");
    }

    [Then(@"the delivered history includes a load with proof of delivery")]
    public void ThenHistoryIncludesAPodLoad()
    {
        using var document = JsonDocument.Parse(state.LastBody!);
        var withPod = document.RootElement.GetProperty("history").EnumerateArray()
            .FirstOrDefault(load => load.GetProperty("podAvailable").GetBoolean());

        withPod.ValueKind.Should().NotBe(JsonValueKind.Undefined, "at least one delivered load must have a POD");
        withPod.GetProperty("status").GetString().Should().Be("Delivered");
        _deliveredLoadId = withPod.GetProperty("id").GetGuid();
    }

    [When(@"the shipper downloads the proof of delivery for that delivered load")]
    public async Task WhenTheShipperDownloadsThePod()
    {
        state.LastResponse = await state.Client.GetAsync($"/api/shipper/loads/{_deliveredLoadId}/pod");
    }

    [Then(@"the download is a PDF")]
    public async Task ThenTheDownloadIsAPdf()
    {
        state.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        state.LastResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");

        var bytes = await state.LastResponse.Content.ReadAsByteArrayAsync();
        bytes.Should().StartWith("%PDF"u8.ToArray());
    }

    [Then(@"the shipper request fails with status (\d+)")]
    public void ThenTheShipperRequestFailsWithStatus(int statusCode)
        => ((int)state.LastResponse!.StatusCode).Should().Be(statusCode, state.LastBody);

    private async Task AuthenticateAsync(string email)
    {
        var login = await state.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = ApplicationDbContextInitialiser.SeedPassword,
        });
        login.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        state.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            document.RootElement.GetProperty("accessToken").GetString());
    }
}
