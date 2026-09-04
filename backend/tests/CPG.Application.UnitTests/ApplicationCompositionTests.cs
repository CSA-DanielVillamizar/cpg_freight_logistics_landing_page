using CPG.Application;
using CPG.Application.Common.Behaviours;
using CPG.Application.Common.Models;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CPG.Application.UnitTests;

public sealed class ApplicationCompositionTests
{
    [Fact]
    public void AddApplication_registers_the_mediator_and_validation_pipeline()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddApplication();

        var provider = services.BuildServiceProvider();

        provider.GetService<ISender>().Should().NotBeNull();

        var behaviours = provider.GetServices<IPipelineBehavior<Ping, string>>().ToList();
        behaviours.Should().Contain(b => b is ValidationBehaviour<Ping, string>);
        behaviours.Should().Contain(b => b is LoggingBehaviour<Ping, string>);
        behaviours.Should().Contain(b => b is PerformanceBehaviour<Ping, string>);
        behaviours.Should().Contain(b => b is UnhandledExceptionBehaviour<Ping, string>);
    }

    [Fact]
    public void Result_failure_carries_errors_and_flips_flags()
    {
        var result = Result.Failure("origin required", "weight required");

        result.Succeeded.Should().BeFalse();
        result.Failed.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    // Minimal request type used only to resolve open generic pipeline behaviours above.
    public sealed record Ping : IRequest<string>;
}
