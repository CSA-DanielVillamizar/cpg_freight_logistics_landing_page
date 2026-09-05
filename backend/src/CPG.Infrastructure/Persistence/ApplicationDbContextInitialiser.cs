using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CPG.Infrastructure.Persistence;

/// <summary>Applies pending migrations and seeds the baseline RBAC users (SPEC.md US-01).</summary>
public sealed class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher)
{
    /// <summary>Default password for every seeded account in non-production environments.</summary>
    public const string SeedPassword = "Passw0rd!";

    private static readonly (string Email, string FullName, UserRole Role)[] SeedUsers =
    [
        ("admin@cpgorlando.com", "Ava Admin", UserRole.Admin),
        ("carrier@cpgorlando.com", "Carl Carrier", UserRole.Carrier),
        ("shipper@cpgorlando.com", "Sam Shipper", UserRole.Shipper),
    ];

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (dbContext.Database.IsRelational())
            {
                await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed");
            throw;
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (email, fullName, role) in SeedUsers)
        {
            var exists = await dbContext.Users
                .AnyAsync(u => u.Email == email, cancellationToken)
                .ConfigureAwait(false);

            if (exists)
            {
                continue;
            }

            dbContext.Users.Add(new User
            {
                Email = email,
                FullName = fullName,
                Role = role,
                PasswordHash = passwordHasher.Hash(SeedPassword),
                IsActive = true,
            });

            logger.LogInformation("Seeded {Role} user {Email}", role, email);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await SeedCarrierAsync(cancellationToken).ConfigureAwait(false);
        await SeedLoadsAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SeedCarrierAsync(CancellationToken cancellationToken)
    {
        const string carrierEmail = "carrier@cpgorlando.com";

        var carrierUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == carrierEmail, cancellationToken)
            .ConfigureAwait(false);

        if (carrierUser is null)
        {
            return;
        }

        var alreadyLinked = await dbContext.Carriers
            .AnyAsync(c => c.UserId == carrierUser.Id, cancellationToken)
            .ConfigureAwait(false);

        if (alreadyLinked)
        {
            return;
        }

        dbContext.Carriers.Add(new Carrier
        {
            CompanyName = "Carl Carrier Heavy Transport LLC",
            UserId = carrierUser.Id,
            DotNumber = "FL-ORL-CAR-001",
            McNumber = "MC-CAR-001",
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Seeded carrier account for {Email}", carrierEmail);
    }

    private async Task SeedLoadsAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Loads.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;

        var loads = new List<Load>
        {
            new()
            {
                Reference = "CPG-48213", ServiceType = ServiceType.ColdChain,
                EquipmentType = "53' Dual-Temp Reefer",
                OriginCity = "Orlando", OriginState = "FL", OriginZip = "32801",
                DestinationCity = "Charlotte", DestinationState = "NC", DestinationZip = "28202",
                DistanceMiles = 543, WeightLbs = 38200, RateUsd = 2140m,
                ShipperName = "Sunbelt Produce Cooperative",
                PickupAtUtc = now.AddDays(3), DeliveryAtUtc = now.AddDays(4),
                TargetTemperatureF = -10, Status = LoadStatus.Available,
                SpecialInstructions = "Continuous temp logging; pre-cooled trailer required.",
            },
            new()
            {
                Reference = "CPG-48217", ServiceType = ServiceType.ColdChain,
                EquipmentType = "Deep-Freeze Flash Trailer",
                OriginCity = "Plant City", OriginState = "FL", OriginZip = "33563",
                DestinationCity = "Miami", DestinationState = "FL", DestinationZip = "33101",
                DistanceMiles = 214, WeightLbs = 41000, RateUsd = 980m,
                ShipperName = "Sunbelt Produce Cooperative",
                PickupAtUtc = now.AddDays(2), DeliveryAtUtc = now.AddDays(2).AddHours(9),
                TargetTemperatureF = -20, Status = LoadStatus.Available,
                SpecialInstructions = "Sub-zero steady pull-down; food-grade sanitation slip.",
            },
            new()
            {
                Reference = "CPG-48220", ServiceType = ServiceType.FdotConcrete,
                EquipmentType = "Self-Offloading Flatbed",
                OriginCity = "Ocala", OriginState = "FL", OriginZip = "34470",
                DestinationCity = "Gainesville", DestinationState = "FL", DestinationZip = "32601",
                DistanceMiles = 41, WeightLbs = 40000, RateUsd = 610m,
                ShipperName = "Florida Infrastructure Corp",
                PickupAtUtc = now.AddDays(4), DeliveryAtUtc = now.AddDays(4).AddHours(5),
                Status = LoadStatus.Available,
                SpecialInstructions = "MASH TL-3 crash-rated units only.",
            },
            new()
            {
                Reference = "CPG-48223", ServiceType = ServiceType.StandardDryVan,
                EquipmentType = "53' Dry Van",
                OriginCity = "Kissimmee", OriginState = "FL", OriginZip = "34741",
                DestinationCity = "Charleston", DestinationState = "SC", DestinationZip = "29401",
                DistanceMiles = 487, WeightLbs = 29900, RateUsd = 1470m,
                ShipperName = "Marcus Sterling Distribution",
                PickupAtUtc = now.AddDays(5), DeliveryAtUtc = now.AddDays(6),
                Status = LoadStatus.Available,
            },
            new()
            {
                Reference = "CPG-48226", ServiceType = ServiceType.ColdChain,
                EquipmentType = "Life Science Transporter",
                OriginCity = "Orlando", OriginState = "FL", OriginZip = "32806",
                DestinationCity = "Raleigh", DestinationState = "NC", DestinationZip = "27601",
                DistanceMiles = 549, WeightLbs = 18700, RateUsd = 2670m,
                ShipperName = "BioCore Pharmaceuticals",
                PickupAtUtc = now.AddHours(-6), DeliveryAtUtc = now.AddHours(6),
                TargetTemperatureF = 0, Status = LoadStatus.InTransit, AssignedCarrierId = carrier?.Id,
                SpecialInstructions = "GDP / 21 CFR Part 11; geofenced deadbolts; chain-of-custody signature.",
            },
            new()
            {
                Reference = "CPG-48231", ServiceType = ServiceType.ColdChain,
                EquipmentType = "Deep-Freeze Flash Trailer",
                OriginCity = "Plant City", OriginState = "FL", OriginZip = "33563",
                DestinationCity = "Atlanta", DestinationState = "GA", DestinationZip = "30301",
                DistanceMiles = 456, WeightLbs = 41000, RateUsd = 2210m,
                ShipperName = "Sunbelt Produce Cooperative",
                PickupAtUtc = now.AddHours(-5), DeliveryAtUtc = now.AddHours(4),
                TargetTemperatureF = -4, Status = LoadStatus.InTransit, AssignedCarrierId = carrier?.Id,
                SpecialInstructions = "Sub-zero frozen produce; door-seal integrity check every checkpoint.",
            },
            new()
            {
                Reference = "CPG-48214", ServiceType = ServiceType.HeavyHaul,
                EquipmentType = "RGN Multi-Axle",
                OriginCity = "Tampa", OriginState = "FL", OriginZip = "33602",
                DestinationCity = "Savannah", DestinationState = "GA", DestinationZip = "31401",
                DistanceMiles = 412, WeightLbs = 96500, RateUsd = 4870m,
                ShipperName = "Gulf Coast Marine & Heavy Civil",
                PickupAtUtc = now.AddDays(1), DeliveryAtUtc = now.AddDays(2),
                Status = LoadStatus.Dispatched, AssignedCarrierId = carrier?.Id,
                SpecialInstructions = "Superload permit escort; pole car front & rear.",
            },
            new()
            {
                Reference = "CPG-48219", ServiceType = ServiceType.HeavyHaul,
                EquipmentType = "Step-Deck / Drop-Deck",
                OriginCity = "Orlando", OriginState = "FL", OriginZip = "32824",
                DestinationCity = "New Orleans", DestinationState = "LA", DestinationZip = "70112",
                DistanceMiles = 655, WeightLbs = 51200, RateUsd = 3120m,
                ShipperName = "Gulf Coast Marine & Heavy Civil",
                PickupAtUtc = now.AddDays(-1), DeliveryAtUtc = now.AddDays(1),
                Status = LoadStatus.InTransit, AssignedCarrierId = carrier?.Id,
                SpecialInstructions = "Over-height 10'2\" cargo; wide/DOT permit corridor.",
            },
            new()
            {
                Reference = "CPG-48216", ServiceType = ServiceType.StandardDryVan,
                EquipmentType = "53' Dry Van",
                OriginCity = "Orlando", OriginState = "FL", OriginZip = "32809",
                DestinationCity = "Atlanta", DestinationState = "GA", DestinationZip = "30301",
                DistanceMiles = 438, WeightLbs = 26400, RateUsd = 1290m,
                ShipperName = "Apex Construction",
                PickupAtUtc = now.AddDays(-4), DeliveryAtUtc = now.AddDays(-3),
                Status = LoadStatus.Delivered, AssignedCarrierId = carrier?.Id,
            },
        };

        dbContext.Loads.AddRange(loads);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Seeded {Count} load board rows", loads.Count);
    }
}

/// <summary>DI + startup helpers for <see cref="ApplicationDbContextInitialiser"/>.</summary>
public static class InitialiserExtensions
{
    public static IServiceCollection AddDatabaseInitialiser(this IServiceCollection services)
    {
        services.AddScoped<ApplicationDbContextInitialiser>();
        return services;
    }

    public static async Task InitialiseDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync(cancellationToken).ConfigureAwait(false);
        await initialiser.SeedAsync(cancellationToken).ConfigureAwait(false);
    }
}
