namespace CPG.Domain.Enums;

/// <summary>Specialized freight service lines offered by CPG Enterprises.</summary>
public enum ServiceType
{
    /// <summary>Temperature-controlled reefer freight (-20&#176;C class lanes).</summary>
    ColdChain = 1,

    /// <summary>Over-dimensional / superload multi-axle transport.</summary>
    HeavyHaul = 2,

    /// <summary>Standard 48'/53' flatbed and step-deck freight.</summary>
    Flatbed = 3,

    /// <summary>FDOT concrete barricade delivery and crane staging.</summary>
    FdotConcrete = 4,

    /// <summary>Standard 48'/53' enclosed dry van freight (Load Board).</summary>
    StandardDryVan = 5,
}
