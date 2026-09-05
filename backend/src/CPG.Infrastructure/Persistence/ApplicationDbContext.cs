using System.Reflection;
using CPG.Application.Common.Interfaces;
using CPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPG.Infrastructure.Persistence;

/// <summary>EF Core context backed by PostgreSQL (SPEC.md section 1 - Infrastructure layer).</summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Carrier> Carriers => Set<Carrier>();

    public DbSet<ComplianceDocument> ComplianceDocuments => Set<ComplianceDocument>();

    public DbSet<Lead> Leads => Set<Lead>();

    public DbSet<Load> Loads => Set<Load>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
