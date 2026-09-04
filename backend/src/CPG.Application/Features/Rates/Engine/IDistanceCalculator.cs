namespace CPG.Application.Features.Rates.Engine;

/// <summary>Estimates driving distance between two US ZIP codes from an in-memory centroid table.</summary>
public interface IDistanceCalculator
{
    /// <summary>Estimated road miles between the two ZIP codes (always &gt;= a sane minimum).</summary>
    double RoadMilesBetween(string originZip, string destinationZip);
}
