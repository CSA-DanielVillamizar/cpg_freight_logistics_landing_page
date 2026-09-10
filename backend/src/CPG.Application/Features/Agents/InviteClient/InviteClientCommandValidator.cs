using FluentValidation;

namespace CPG.Application.Features.Agents.InviteClient;

public sealed class InviteClientCommandValidator : AbstractValidator<InviteClientCommand>
{
    public InviteClientCommandValidator()
    {
        RuleFor(x => x.InvitedEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}
