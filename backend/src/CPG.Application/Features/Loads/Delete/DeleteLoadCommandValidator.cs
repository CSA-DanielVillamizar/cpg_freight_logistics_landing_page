using FluentValidation;

namespace CPG.Application.Features.Loads.Delete;

public sealed class DeleteLoadCommandValidator : AbstractValidator<DeleteLoadCommand>
{
    public DeleteLoadCommandValidator()
    {
        RuleFor(x => x.LoadId).NotEmpty();
    }
}
