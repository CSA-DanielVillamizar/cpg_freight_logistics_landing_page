using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CPG.Api.IntegrationTests.Support;
using CPG.Application.Features.Authentication;
using CPG.Infrastructure.Persistence;
using FluentAssertions;
using Reqnroll;

namespace CPG.Api.IntegrationTests.StepDefinitions;

[Binding]
public sealed class BillingStepDefinitions(ScenarioState state)
{
    private Guid _loadId;
    private Guid _invoiceId;
    private string _checkoutUrl = string.Empty;

    [Given(@"a carrier is authenticated for billing")]
    public async Task GivenACarrierIsAuthenticated()
        => await AuthenticateAsync("carrier@cpgorlando.com");

    [Given(@"the carrier has the in-transit load ""(.*)""")]
    public async Task GivenTheCarrierHasAnInTransitLoad(string reference)
    {
        var response = await state.Client.GetAsync("/api/loads?status=InTransit");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var load = document.RootElement.EnumerateArray()
            .First(row => row.GetProperty("reference").GetString() == reference);
        _loadId = load.GetProperty("id").GetGuid();
    }

    [When(@"the carrier marks the load delivered")]
    public async Task WhenTheCarrierMarksTheLoadDelivered()
    {
        state.LastResponse = await state.Client.PostAsync($"/api/loads/{_loadId}/deliver", content: null);
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
        state.LastResponse.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);
    }

    [Then(@"an invoice for ""(.*)"" is raised for the shipper")]
    public async Task ThenAnInvoiceIsRaised(string loadReference)
    {
        await AuthenticateAsync("shipper@cpgorlando.com");

        var raised = await TestScope.EventuallyAsync(
            async () =>
            {
                var response = await state.Client.GetAsync("/api/shipper/invoices");
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return document.RootElement.GetProperty("invoices").EnumerateArray()
                    .Any(invoice => invoice.GetProperty("loadReference").GetString() == loadReference);
            },
            TimeSpan.FromSeconds(20));

        raised.Should().BeTrue("the LoadDelivered integration event must be consumed and an invoice raised");

        _invoiceId = await FindInvoiceIdAsync(loadReference);
    }

    [When(@"the shipper starts a checkout for that invoice")]
    public async Task WhenTheShipperStartsCheckout()
    {
        state.LastResponse = await state.Client.PostAsync($"/api/shipper/invoices/{_invoiceId}/pay", content: null);
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();
    }

    [Then(@"a checkout URL is returned")]
    public void ThenACheckoutUrlIsReturned()
    {
        state.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK, state.LastBody);
        using var document = JsonDocument.Parse(state.LastBody!);
        _checkoutUrl = document.RootElement.GetProperty("checkoutUrl").GetString()!;
        _checkoutUrl.Should().Contain("cs_mock_");
    }

    [When(@"Stripe confirms the checkout completed")]
    public async Task WhenStripeConfirmsCheckoutCompleted()
    {
        var sessionId = new Uri(_checkoutUrl).AbsolutePath.Split('/')[^1];

        var payload = JsonSerializer.Serialize(new
        {
            type = "checkout.session.completed",
            data = new { @object = new { id = sessionId } },
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/stripe")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("Stripe-Signature", "whsec_cpg_mock");

        var response = await state.Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"the invoice for ""(.*)"" is Paid")]
    public async Task ThenTheInvoiceIsPaid(string loadReference)
    {
        var paid = await TestScope.EventuallyAsync(
            async () =>
            {
                var response = await state.Client.GetAsync("/api/shipper/invoices");
                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return document.RootElement.GetProperty("invoices").EnumerateArray()
                    .Any(invoice => invoice.GetProperty("loadReference").GetString() == loadReference
                        && invoice.GetProperty("status").GetString() == "Paid");
            },
            TimeSpan.FromSeconds(10));

        paid.Should().BeTrue();
    }

    private async Task<Guid> FindInvoiceIdAsync(string loadReference)
    {
        var response = await state.Client.GetAsync("/api/shipper/invoices");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("invoices").EnumerateArray()
            .First(invoice => invoice.GetProperty("loadReference").GetString() == loadReference)
            .GetProperty("id").GetGuid();
    }

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
