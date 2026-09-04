using CPG.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CPG.Api.IntegrationTests.Support;

/// <summary>Convenience helpers to reach into the running test host's DI container.</summary>
public static class TestScope
{
    public static async Task WithDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = TestApp.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(dbContext);
    }

    public static async Task<TResult> WithDbContextAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> action)
    {
        using var scope = TestApp.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(dbContext);
    }

    public static async Task<TResult> WithServiceAsync<TService, TResult>(Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        using var scope = TestApp.Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return await action(service);
    }

    /// <summary>Polls <paramref name="condition"/> until it is true or the timeout elapses.</summary>
    public static async Task<bool> EventuallyAsync(
        Func<Task<bool>> condition,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(500);
        var deadline = DateTimeOffset.UtcNow + timeout;

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await condition())
            {
                return true;
            }

            await Task.Delay(interval);
        }

        return await condition();
    }
}
