using FluentValidation;

namespace CPG.Application.Features.Billing.CreateCheckout;

public sealed class CreateInvoiceCheckoutCommandValidator : AbstractValidator<CreateInvoiceCheckoutCommand>
{
    public CreateInvoiceCheckoutCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.SuccessUrl).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.CancelUrl).NotEmpty().MaximumLength(2048);
    }
}
