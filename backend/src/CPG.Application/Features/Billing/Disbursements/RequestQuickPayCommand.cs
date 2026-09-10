using MediatR;

namespace CPG.Application.Features.Billing.Disbursements;

/// <summary>
/// The assigned Carrier opts into Quick Pay for a not-yet-paid invoice, trading a fee for
/// immediate settlement instead of waiting for the shipper's net-30 terms (T-SDD Epica 2B).
/// </summary>
public sealed record RequestQuickPayCommand(Guid InvoiceId) : IRequest;
