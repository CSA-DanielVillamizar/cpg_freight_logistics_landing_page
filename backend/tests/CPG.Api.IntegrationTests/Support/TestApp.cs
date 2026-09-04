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
        // Docker Desktop can be slow/flaky under load; retry the container bring-up once.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await StartContainersAsync();
                break;
            }
            catch (Exception) when (attempt < 2)
            {
                await SafeStopAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        Factory = new CpgApiFactory(
            _postgres!.GetConnectionString(),
            _rabbitMq!.GetConnectionString(),
            _azurite!.GetConnectionString());

        // Force host build so migrations + user seeding run before the first scenario.
        _ = Factory.Services;
    }

    public static Task StopAsync() => SafeStopAsync();

    private static async Task StartContainersAsync()
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

        await _postgres.StartAsync();
        await _azurite.StartAsync();
        await _rabbitMq.StartAsync();
    }

    private static async Task SafeStopAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        foreach (var container in new IAsyncDisposable?[] { _rabbitMq, _azurite, _postgres })
        {
            if (container is not null)
            {
                try
                {
                    await container.DisposeAsync();
                }
                catch
                {
                    // best effort teardown
                }
            }
        }

        _rabbitMq = null;
        _azurite = null;
        _postgres = null;
    }
}
