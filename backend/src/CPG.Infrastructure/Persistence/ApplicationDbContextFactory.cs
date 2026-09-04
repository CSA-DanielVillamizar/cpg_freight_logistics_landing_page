using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CPG.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so <c>dotnet ef migrations</c> can build the model without booting
/// the API host. Connection string comes from <c>CPG_ConnectionStrings__Postgres</c> or a
/// local default matching docker-compose.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("CPG_ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=cpg;Username=cpg;Password=cpg_local_dev";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContextFactory).Assembly.FullName))
            .Options;

        return new ApplicationDbContext(options);
    }
}
