using CPG.Domain.Common;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using CPG.Domain.Events;
using FluentAssertions;
using Xunit;

namespace CPG.Domain.UnitTests;

public sealed class LoadLifecycleTests
{
    private static Load NewAvailableLoad() => new()
    {
        Reference = "CPG-90001",
        ServiceType = ServiceType.StandardDryVan,
        EquipmentType = "53' Dry Van",
        OriginCity = "Orlando",
        OriginState = "FL",
        OriginZip = "32801",
        DestinationCity = "Atlanta",
        DestinationState = "GA",
        DestinationZip = "30301",
        DistanceMiles = 438,
        WeightLbs = 30000,
        RateUsd = 1800m,
        ShipperName = "E2E Freight Co.",
        PickupAtUtc = DateTimeOffset.UtcNow.AddDays(1),
        DeliveryAtUtc = DateTimeOffset.UtcNow.AddDays(2),
    };

    [Fact]
    public void Accept_moves_an_available_load_to_dispatched_and_raises_an_event()
    {
        var load = NewAvailableLoad();
        var carrierId = Guid.NewGuid();

        load.Accept(carrierId);

        load.Status.Should().Be(LoadStatus.Dispatched);
        load.AssignedCarrierId.Should().Be(carrierId);
        load.DomainEvents.OfType<LoadAcceptedDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void MarkInTransit_moves_a_dispatched_load_to_in_transit()
    {
        var load = NewAvailableLoad();
        load.Accept(Guid.NewGuid());

        load.MarkInTransit();

        load.Status.Should().Be(LoadStatus.InTransit);
    }

    [Fact]
    public void MarkInTransit_from_available_is_rejected()
    {
        var load = NewAvailableLoad();

        var act = load.MarkInTransit;

        act.Should().Throw<DomainException>().WithMessage("*must be Dispatched*");
    }

    [Fact]
    public void MarkInTransit_is_not_repeatable()
    {
        var load = NewAvailableLoad();
        load.Accept(Guid.NewGuid());
        load.MarkInTransit();

        var act = load.MarkInTransit;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Delivered_load_raises_the_billing_event_with_the_shipper()
    {
        var shipperUserId = Guid.NewGuid();
        var load = NewAvailableLoad();
        load.ShipperUserId = shipperUserId;
        load.Accept(Guid.NewGuid());
        load.MarkInTransit();

        load.MarkDelivered();

        load.Status.Should().Be(LoadStatus.Delivered);
        load.DomainEvents.OfType<LoadDeliveredDomainEvent>()
            .Should().ContainSingle()
            .Which.ShipperUserId.Should().Be(shipperUserId);
    }

    [Fact]
    public void Full_lifecycle_available_to_delivered_is_valid()
    {
        var load = NewAvailableLoad();

        load.Accept(Guid.NewGuid());
        load.MarkInTransit();
        load.MarkDelivered();

        load.Status.Should().Be(LoadStatus.Delivered);
    }
}
