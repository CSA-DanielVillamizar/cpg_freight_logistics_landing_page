using FluentValidation;

namespace CPG.Application.Features.Loads.Accept;

public sealed class AcceptLoadCommandValidator : AbstractValidator<AcceptLoadCommand>
{
    public AcceptLoadCommandValidator()
    {
        RuleFor(x => x.LoadId).NotEmpty();
    }
}
