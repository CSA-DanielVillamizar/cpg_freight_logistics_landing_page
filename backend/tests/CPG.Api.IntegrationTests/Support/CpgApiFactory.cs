using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CPG.Api.IntegrationTests.Support;

/// <summary>
/// Hosts the API in-process for integration tests, pointing configuration at the
/// disposable containers from <see cref="ContainerEnvironment"/>.
/// </summary>
public sealed class CpgApiFactory(
    string postgresConnectionString,
    string rabbitMqConnectionString,
    string azuriteConnectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = postgresConnectionString,
                ["ConnectionStrings:RabbitMq"] = rabbitMqConnectionString,
                ["BlobStorage:Provider"] = "Azure",
                ["BlobStorage:ConnectionString"] = azuriteConnectionString,
                ["Jwt:SigningKey"] = "integration-tests-cpg-signing-key-0123456789abcdef",
                ["Jwt:AccessTokenMinutes"] = "15",
            });
        });
    }
}
