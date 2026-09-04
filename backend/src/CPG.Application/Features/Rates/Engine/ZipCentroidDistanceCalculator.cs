namespace CPG.Application.Features.Rates.Engine;

/// <summary>
/// ZIP3-centroid + haversine distance with a road-circuity factor. Covers Florida and the
/// major south-east / national lanes CPG runs; unknown prefixes fall back to a numeric
/// heuristic. All lookups are O(1) dictionary hits - no I/O.
/// </summary>
public sealed class ZipCentroidDistanceCalculator : IDistanceCalculator
{
    private const double EarthRadiusMiles = 3958.7613;
    private const double RoadCircuityFactor = 1.18;
    private const double MinimumMiles = 40d;

    // ZIP3 prefix -> approximate (latitude, longitude) centroid.
    private static readonly Dictionary<string, (double Lat, double Lon)> Centroids =
        new(StringComparer.Ordinal)
        {
            // Florida
            ["320"] = (30.33, -81.66), // Jacksonville
            ["321"] = (28.35, -80.73), // Cocoa / Space Coast
            ["322"] = (29.65, -81.60), // Palatka / St. Augustine
            ["323"] = (30.44, -84.28), // Tallahassee
            ["324"] = (30.19, -85.66), // Panama City
            ["325"] = (30.42, -87.22), // Pensacola
            ["326"] = (29.65, -82.33), // Gainesville
            ["327"] = (28.80, -81.27), // Sanford
            ["328"] = (28.54, -81.38), // Orlando
            ["329"] = (28.29, -81.41), // Kissimmee
            ["330"] = (25.78, -80.20), // Miami
            ["331"] = (25.78, -80.20), // Miami
            ["332"] = (25.94, -80.24), // Miami (north)
            ["333"] = (26.14, -80.14), // Fort Lauderdale
            ["334"] = (26.71, -80.06), // West Palm Beach
            ["335"] = (27.95, -82.46), // Tampa
            ["336"] = (27.95, -82.46), // Tampa
            ["337"] = (27.77, -82.64), // St. Petersburg
            ["338"] = (28.05, -81.95), // Lakeland
            ["339"] = (26.64, -81.87), // Fort Myers
            ["341"] = (26.14, -81.79), // Naples
            ["342"] = (27.34, -82.53), // Sarasota
            ["344"] = (29.19, -82.14), // Ocala
            ["347"] = (28.54, -81.38), // Orlando (secondary)
            ["349"] = (27.20, -80.25), // Fort Pierce / Stuart

            // South-east corridor
            ["300"] = (33.75, -84.39), // Atlanta
            ["303"] = (33.75, -84.39), // Atlanta
            ["308"] = (33.47, -82.01), // Augusta
            ["310"] = (32.08, -81.09), // Savannah
            ["313"] = (32.08, -81.09), // Savannah / Statesboro
            ["294"] = (32.78, -79.93), // Charleston
            ["282"] = (35.23, -80.84), // Charlotte
            ["352"] = (33.52, -86.81), // Birmingham
            ["372"] = (36.16, -86.78), // Nashville
            ["381"] = (35.15, -90.05), // Memphis
            ["700"] = (29.95, -90.07), // New Orleans

            // Wider national reference points
            ["770"] = (29.76, -95.37), // Houston
            ["100"] = (40.71, -74.01), // New York
            ["600"] = (41.88, -87.63), // Chicago
            ["900"] = (34.05, -118.24), // Los Angeles
        };

    public double RoadMilesBetween(string originZip, string destinationZip)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originZip);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationZip);

        var origin = Prefix(originZip);
        var destination = Prefix(destinationZip);

        if (Centroids.TryGetValue(origin, out var a) && Centroids.TryGetValue(destination, out var b))
        {
            var straightLine = Haversine(a.Lat, a.Lon, b.Lat, b.Lon);
            return Math.Max(MinimumMiles, straightLine * RoadCircuityFactor);
        }

        // Fallback: coarse numeric distance between prefixes, bounded to a sane range.
        var delta = Math.Abs(ToInt(origin) - ToInt(destination));
        return Math.Clamp(delta * 12d, 75d, 2800d);
    }

    private static string Prefix(string zip)
    {
        var trimmed = zip.Trim();
        return trimmed[..Math.Min(3, trimmed.Length)];
    }

    private static int ToInt(string prefix) => int.TryParse(prefix, out var value) ? value : 500;

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var h = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2))
            + (Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2));
        return EarthRadiusMiles * 2 * Math.Asin(Math.Min(1d, Math.Sqrt(h)));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180d;
}
