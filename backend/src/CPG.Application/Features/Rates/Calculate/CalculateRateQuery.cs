using CPG.Domain.Enums;
using MediatR;

namespace CPG.Application.Features.Rates.Calculate;

/// <summary>Query: compute a specialized-freight rate breakdown (SPEC.md US-02).</summary>
public sealed record CalculateRateQuery(
    ServiceType ServiceType,
    string OriginZip,
    string DestinationZip,
    int WeightLbs,
    decimal? TargetTemperatureCelsius) : IRequest<RateCalculationResponse>
{
    public static CalculateRateQuery FromRequest(RateCalculationRequest request) => new(
        request.ServiceType,
        request.OriginZip,
        request.DestinationZip,
        request.WeightLbs,
        request.TargetTemperatureCelsius);

    public RateCalculationRequest ToRequest() => new()
    {
        ServiceType = ServiceType,
        OriginZip = OriginZip,
        DestinationZip = DestinationZip,
        WeightLbs = WeightLbs,
        TargetTemperatureCelsius = TargetTemperatureCelsius,
    };
}
