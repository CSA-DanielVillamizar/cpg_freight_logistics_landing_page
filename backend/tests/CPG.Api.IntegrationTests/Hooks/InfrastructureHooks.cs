using CPG.Api.IntegrationTests.Support;
using Reqnroll;

namespace CPG.Api.IntegrationTests.Hooks;

/// <summary>Brings the shared Testcontainers stack up once per test run.</summary>
[Binding]
public static class InfrastructureHooks
{
    [BeforeTestRun]
    public static Task StartInfrastructureAsync() => TestApp.StartAsync();

    [AfterTestRun]
    public static Task StopInfrastructureAsync() => TestApp.StopAsync();
}
