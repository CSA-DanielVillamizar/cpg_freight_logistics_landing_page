using MediatR;

namespace CPG.Application.Features.Loads.Delete;

/// <summary>
/// Administrative logical delete of a load. Sets <c>IsDeleted</c> (never a hard
/// <c>DbSet.Remove()</c>), cancels the associated shipper invoice if one exists and is not
/// paid, and writes a mandatory <c>LoadDeleted</c> audit row. Restricted to <c>AdminOnly</c>.
/// </summary>
public sealed record DeleteLoadCommand(Guid LoadId) : IRequest;
