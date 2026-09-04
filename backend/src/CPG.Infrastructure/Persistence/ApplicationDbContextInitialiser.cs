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
