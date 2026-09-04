using Testcontainers.Azurite;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace CPG.Api.IntegrationTests.Support;

/// <summary>
/// Test-run-wide fixture: one set of disposable PostgreSQL / RabbitMQ / Azurite containers
/// and a single <see cref="CpgApiFactory"/> hosting the API against them. Started/stopped by
/// <see cref="Hooks.InfrastructureHooks"/> so every Reqnroll scenario in the run shares it.
/// </summary>
public static class TestApp
{
    private static PostgreSqlContainer? _postgres;
    private static RabbitMqContainer? _rabbitMq;
    private static AzuriteContainer? _azurite;

    public static CpgApiFactory Factory { get; private set; } = default!;

    public static async Task StartAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("cpg")
            .WithUsername("cpg")
            .WithPassword("cpg_local_dev")
            .Build();

        _rabbitMq = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-management-alpine")
            .Build();

        _azurite = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:3.33.0")
            .Build();

        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync(), _azurite.StartAsync());

        Factory = new CpgApiFactory(
            _postgres.GetConnectionString(),
            _rabbitMq.GetConnectionString(),
            _azurite.GetConnectionString());

        // Force host build so migrations + user seeding run before the first scenario.
        _ = Factory.Services;
    }

    public static async Task StopAsync()
    {
        await Factory.DisposeAsync();
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }

        if (_rabbitMq is not null)
        {
            await _rabbitMq.DisposeAsync();
        }

        if (_azurite is not null)
        {
            await _azurite.DisposeAsync();
        }
    }
}
