using CPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Common.Interfaces;

/// <summary>
/// Persistence abstraction consumed by CQRS handlers. Implemented by the EF Core
/// <c>ApplicationDbContext</c> in the Infrastructure layer (PostgreSQL).
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Carrier> Carriers { get; }

    DbSet<ComplianceDocument> ComplianceDocuments { get; }

    DbSet<Lead> Leads { get; }

    DbSet<Load> Loads { get; }

    DbSet<Invoice> Invoices { get; }

    DbSet<AuditLogEntry> AuditLogEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
