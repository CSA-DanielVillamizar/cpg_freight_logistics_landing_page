using FluentValidation;

namespace CPG.Application.Features.Telemetry.RegisterDevice;

public sealed class RegisterTelemetryDeviceCommandValidator : AbstractValidator<RegisterTelemetryDeviceCommand>
{
    public RegisterTelemetryDeviceCommandValidator()
    {
        RuleFor(x => x.Provider).IsInEnum();

        RuleFor(x => x.ExternalDeviceId)
            .NotEmpty()
            .MaximumLength(120);
    }
}
