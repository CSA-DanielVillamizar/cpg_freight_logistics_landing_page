using FluentValidation;

namespace CPG.Application.Features.Loads.Depart;

public sealed class MarkLoadInTransitCommandValidator : AbstractValidator<MarkLoadInTransitCommand>
{
    public MarkLoadInTransitCommandValidator()
    {
        RuleFor(x => x.LoadId).NotEmpty();
    }
}
