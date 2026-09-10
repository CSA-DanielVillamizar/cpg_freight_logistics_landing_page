using System.Diagnostics;
using System.Text.Json;
using CPG.Application.Common.Exceptions;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using CPG.Domain.Entities;
using CPG.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Features.Loads.Create;

public sealed class CreateLoadCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    IDisbursementCalculator disbursementCalculator)
    : IRequestHandler<CreateLoadCommand, LoadSummaryResponse>
{
    public async Task<LoadSummaryResponse> Handle(
        CreateLoadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenAccessException("The request is not authenticated.");

        // An Agent publishes on behalf of one of their own clients; a Shipper always books
        // their own freight; an Admin posts on any shipper's behalf.
        Agent? agent = null;
        if (currentUser.Role == UserRole.Agent)
        {
            agent = await dbContext.Agents
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException("An agent profile is required to publish loads.");

            if (request.ShipperUserId is null)
            {
                throw new DomainException("An Agent must specify which client (shipperUserId) this load is for.");
            }
        }

        var shipperUserId = currentUser.Role == UserRole.Shipper
            ? userId
            : request.ShipperUserId;

        var reference = string.IsNullOrWhiteSpace(request.Reference)
            ? $"CPG-{Random.Shared.Next(10_000, 100_000)}"
            : request.Reference.Trim();

        var referenceTaken = await dbContext.Loads
            .IgnoreQueryFilters()
            .AnyAsync(l => l.Reference == reference, cancellationToken)
            .ConfigureAwait(false);

        if (referenceTaken)
        {
            throw new DomainException($"A load with reference {reference} already exists.");
        }

        var load = new Load
        {
            Reference = reference,
            ServiceType = request.ServiceType,
            EquipmentType = request.EquipmentType,
            OriginCity = request.OriginCity,
            OriginState = request.OriginState.ToUpperInvariant(),
            OriginZip = request.OriginZip,
            DestinationCity = request.DestinationCity,
            DestinationState = request.DestinationState.ToUpperInvariant(),
            DestinationZip = request.DestinationZip,
            DistanceMiles = request.DistanceMiles,
            WeightLbs = request.WeightLbs,
            RateUsd = request.RateUsd,
            ShipperName = request.ShipperName,
            ShipperUserId = shipperUserId,
            PickupAtUtc = request.PickupAtUtc,
            DeliveryAtUtc = request.DeliveryAtUtc,
            TargetTemperatureF = request.TargetTemperatureF,
            SpecialInstructions = request.SpecialInstructions,
            Status = LoadStatus.Available,
        };

        if (agent is not null)
        {
            var breakdown = disbursementCalculator.Calculate(new DisbursementCalculationRequest
            {
                GrossAmountUsd = load.RateUsd,
                AgentCommissionRatePercent = agent.CommissionRatePercent,
                QuickPayRequested = false,
            });

            load.AttributeToAgent(agent.Id, breakdown.AgentCommissionAmountUsd);
        }

        dbContext.Loads.Add(load);
        dbContext.AuditLogEntries.Add(new AuditLogEntry
        {
            Action = "LoadCreated",
            EntityName = nameof(Load),
            EntityId = load.Id.ToString(),
            UserId = userId.ToString(),
            TimestampUtc = clock.UtcNow,
            TraceId = Activity.Current?.TraceId.ToString(),
            DataJson = JsonSerializer.Serialize(new
            {
                load.Reference,
                load.RateUsd,
                load.ShipperUserId,
                load.AgentId,
                PostedByRole = currentUser.Role?.ToString(),
            }),
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return LoadSummaryResponse.FromEntity(load);
    }
}
