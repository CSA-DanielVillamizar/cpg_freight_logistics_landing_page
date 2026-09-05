using Azure.Identity;
using CPG.Application.Common.Interfaces;
using CPG.Infrastructure.Identity;
using CPG.Infrastructure.Messaging;
using CPG.Infrastructure.Persistence;
using CPG.Infrastructure.Persistence.Interceptors;
using CPG.Infrastructure.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CPG.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer. Transport and storage adapters switch
/// between local-dev defaults (RabbitMQ, Azurite connection string) and Azure-native,
/// passwordless managed-identity clients based on configuration.
/// </summary>
public static class DependencyInjection
{
    internal const string DefaultPostgres =
        "Host=localhost;Port=5432;Database=cpg;Username=cpg;Password=cpg_local_dev";

    internal const string DefaultRabbitMq = "amqp://cpg:cpg_local_dev@localhost:5672";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        AddPersistence(services);
        AddMessaging(services, configuration);
        AddBlobStorage(services, configuration);
        AddSecurity(services, configuration);

        services.AddSingleton<IStripePaymentService, Billing.MockStripePaymentService>();
        services.AddSingleton<IDateTimeProvider, Services.DateTimeProvider>();
        services.AddDatabaseInitialiser();

        return services;
    }

    // Configuration is read lazily from the container so WebApplicationFactory overrides
    // (which land only after the host is built) are honoured in integration tests.
    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>()
                .GetConnectionString("Postgres") ?? DefaultPostgres;

            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IIdempotencyService, IdempotencyService>();
    }

    /// <summary>
    /// Selects the MassTransit transport: Azure Service Bus when
    /// <c>ServiceBus:FullyQualifiedNamespace</c> or <c>ConnectionStrings:ServiceBus</c> is
    /// present (production), otherwise RabbitMQ (local development / integration tests).
    /// The transport choice is a registration-time decision, so it is read eagerly here;
    /// environment variables are already loaded by this point in a real host.
    /// </summary>
    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        var serviceBusNamespace = configuration["ServiceBus:FullyQualifiedNamespace"];
        var serviceBusConnectionString = configuration.GetConnectionString("ServiceBus");
        var useServiceBus = !string.IsNullOrWhiteSpace(serviceBusNamespace)
            || !string.IsNullOrWhiteSpace(serviceBusConnectionString);

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            bus.AddConsumer<ComplianceNotificationConsumer>();
            bus.AddConsumer<LeadNotificationConsumer>();
            bus.AddConsumer<LoadAcceptedNotificationConsumer>();
            bus.AddConsumer<LoadDeliveredNotificationConsumer>();

            if (useServiceBus)
            {
                bus.UsingAzureServiceBus((context, cfg) =>
                {
                    var config = context.GetRequiredService<IConfiguration>();
                    var connectionString = config.GetConnectionString("ServiceBus");
                    var fqNamespace = config["ServiceBus:FullyQualifiedNamespace"];

                    if (!string.IsNullOrWhiteSpace(fqNamespace))
                    {
                        // Passwordless: managed identity (AZURE_CLIENT_ID selects the UAMI).
                        var clientId = config["AZURE_CLIENT_ID"];
                        var credential = string.IsNullOrWhiteSpace(clientId)
                            ? new DefaultAzureCredential()
                            : new DefaultAzureCredential(new DefaultAzureCredentialOptions
                            {
                                ManagedIdentityClientId = clientId,
                            });

                        cfg.Host($"sb://{fqNamespace}", host => host.TokenCredential = credential);
                    }
                    else
                    {
                        cfg.Host(connectionString);
                    }

                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                bus.UsingRabbitMq((context, cfg) =>
                {
                    var connectionString = context.GetRequiredService<IConfiguration>()
                        .GetConnectionString("RabbitMq") ?? DefaultRabbitMq;

                    cfg.Host(new Uri(connectionString));
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();
    }

    private static void AddBlobStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BlobStorageOptions>()
            .Bind(configuration.GetSection(BlobStorageOptions.SectionName));

        services.AddSingleton<IBlobStorage>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<BlobStorageOptions>>();
            return string.Equals(options.Value.Provider, "Local", StringComparison.OrdinalIgnoreCase)
                ? new LocalFileSystemBlobStorage(options)
                : new AzureBlobStorageService(options);
        });
    }

    private static void AddSecurity(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
    }
}
