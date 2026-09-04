using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CPG.Api.IntegrationTests.Support;
using CPG.Application.Features.Rates;
using CPG.Domain.Enums;
using FluentAssertions;
using Reqnroll;

namespace CPG.Api.IntegrationTests.StepDefinitions;

[Binding]
public sealed class RateCalculationStepDefinitions(ScenarioState state)
{
    private static readonly IReadOnlyDictionary<string, string> CityToZip = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Miami, FL"] = "33101",
        ["Orlando, FL"] = "32801",
        ["Tampa, FL"] = "33602",
        ["Jacksonville, FL"] = "32202",
        ["Atlanta, GA"] = "30301",
    };

    private ServiceType _serviceType;
    private string _originZip = string.Empty;
    private string _destinationZip = string.Empty;
    private int _weightLbs;
    private decimal? _targetTemperatureCelsius;
    private double? _computeMs;

    [Given(@"a Shipper requests a rate calculation for service type ""(.*)""")]
    public void GivenAShipperRequestsARateCalculationForServiceType(string serviceType)
    {
        _serviceType = serviceType.Replace(" ", string.Empty, StringComparison.Ordinal) switch
        {
            "ColdChain" => ServiceType.ColdChain,
            "HeavyHaul" => ServiceType.HeavyHaul,
            "Flatbed" => ServiceType.Flatbed,
            "FDOTConcrete" or "FdotConcrete" => ServiceType.FdotConcrete,
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unknown service type"),
        };
    }

    [Given(@"origin is ""(.*)"" and destination is ""(.*)""")]
    public void GivenOriginAndDestination(string origin, string destination)
    {
        _originZip = CityToZip[origin];
        _destinationZip = CityToZip[destination];
    }

    [Given(@"cargo weight is (\d+) lbs with target temperature of (-?\d+) degrees Celsius")]
    public void GivenCargoWeightAndTemperature(int weightLbs, int temperatureCelsius)
    {
        _weightLbs = weightLbs;
        _targetTemperatureCelsius = temperatureCelsius;
    }

    [When(@"the client invokes POST ""(.*)""")]
    public async Task WhenTheClientInvokesPost(string path)
    {
        var payload = new RateCalculationRequest
        {
            ServiceType = _serviceType,
            OriginZip = _originZip,
            DestinationZip = _destinationZip,
            WeightLbs = _weightLbs,
            TargetTemperatureCelsius = _targetTemperatureCelsius,
        };

        // Warm the HTTP path so the measured request reflects steady-state latency.
        using (await state.Client.PostAsJsonAsync(path, payload))
        {
        }

        state.LastResponse = await state.Client.PostAsJsonAsync(path, payload);
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();

        if (state.LastResponse.Headers.TryGetValues("X-Rate-Compute-Ms", out var values)
            && double.TryParse(values.FirstOrDefault(), NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
        {
            _computeMs = ms;
        }
    }

    [Then(@"the system should return HTTP status (\d+) OK")]
    public void ThenTheSystemShouldReturnHttpStatusOk(int statusCode)
    {
        state.LastResponse.Should().NotBeNull();
        ((int)state.LastResponse!.StatusCode).Should().Be(statusCode);
        state.LastResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"the computation time must be less than (\d+) milliseconds")]
    public void ThenTheComputationTimeMustBeLessThan(int budgetMs)
    {
        _computeMs.Should().NotBeNull("the API must report X-Rate-Compute-Ms");
        _computeMs!.Value.Should().BeLessThan(budgetMs);
    }

    [Then(@"the response must break down base rate, cold chain surcharge, and fuel surcharge")]
    public void ThenTheResponseMustBreakDownTheRate()
    {
        state.LastBody.Should().NotBeNullOrWhiteSpace();

        using var document = JsonDocument.Parse(state.LastBody!);
        var root = document.RootElement;

        var baseRate = root.GetProperty("baseRate").GetDecimal();
        var coldChain = root.GetProperty("coldChainSurcharge").GetDecimal();
        var fuel = root.GetProperty("fuelSurcharge").GetDecimal();
        var total = root.GetProperty("totalEstimated").GetDecimal();

        baseRate.Should().BeGreaterThan(0m);
        fuel.Should().BeGreaterThan(0m);
        coldChain.Should().BeGreaterThan(0m, "the shipment targets -20 degrees Celsius");
        total.Should().BeApproximately(baseRate + coldChain + fuel, 0.01m);
        root.GetProperty("currency").GetString().Should().Be("USD");
    }
}
