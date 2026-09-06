using CPG.Domain.Common;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CPG.Domain.UnitTests;

public sealed class InvoiceTests
{
    private static Invoice PendingInvoice()
    {
        var load = new Load
        {
            Reference = "CPG-70001",
            ServiceType = ServiceType.Flatbed,
            EquipmentType = "48' Flatbed",
            OriginCity = "Tampa",
            OriginState = "FL",
            OriginZip = "33602",
            DestinationCity = "Mobile",
            DestinationState = "AL",
            DestinationZip = "36602",
            DistanceMiles = 430,
            WeightLbs = 44000,
            RateUsd = 2600m,
            ShipperName = "Gulf Coast Builders",
            ShipperUserId = Guid.NewGuid(),
            PickupAtUtc = DateTimeOffset.UtcNow,
            DeliveryAtUtc = DateTimeOffset.UtcNow.AddDays(1),
        };

        return Invoice.ForDeliveredLoad(load, "INV-70001", DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Delivered_load_invoice_starts_pending_net_30()
    {
        var invoice = PendingInvoice();

        invoice.Status.Should().Be(InvoiceStatus.Pending);
        invoice.DueDate.Should().BeCloseTo(invoice.IssuedAtUtc.AddDays(30), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Cancel_voids_a_pending_invoice()
    {
        var invoice = PendingInvoice();

        invoice.Cancel();

        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Cancel_is_idempotent()
    {
        var invoice = PendingInvoice();

        invoice.Cancel();
        var act = invoice.Cancel;

        act.Should().NotThrow();
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void A_paid_invoice_cannot_be_cancelled()
    {
        var invoice = PendingInvoice();
        invoice.MarkPaid(DateTimeOffset.UtcNow);

        var act = invoice.Cancel;

        act.Should().Throw<DomainException>().WithMessage("*paid*");
    }

    [Fact]
    public void Load_and_invoice_are_soft_deletable()
    {
        typeof(Load).Should().BeAssignableTo<ISoftDelete>();
        typeof(Invoice).Should().BeAssignableTo<ISoftDelete>();
    }
}
