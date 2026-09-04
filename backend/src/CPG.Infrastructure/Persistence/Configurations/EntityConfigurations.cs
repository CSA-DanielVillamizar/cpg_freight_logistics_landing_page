using CPG.Domain.Common;
using CPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CPG.Infrastructure.Persistence.Configurations;

internal static class RowVersionConfigurationExtensions
{
    /// <summary>
    /// Maps <see cref="IHasRowVersion.RowVersion"/> onto the PostgreSQL system column
    /// <c>xmin</c> as an optimistic-concurrency token (SPEC.md section 2).
    /// </summary>
    public static void MapXminRowVersion<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IHasRowVersion
    {
        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(32);
        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(u => u.DomainEvents);
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Token).HasMaxLength(512).IsRequired();
        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.Ignore(rt => rt.IsActive);
    }
}

internal sealed class CarrierConfiguration : IEntityTypeConfiguration<Carrier>
{
    public void Configure(EntityTypeBuilder<Carrier> builder)
    {
        builder.ToTable("carriers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.DotNumber).HasMaxLength(32);
        builder.Property(c => c.McNumber).HasMaxLength(32);
        builder.Property(c => c.ComplianceStatus).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(c => c.UserId).IsUnique();
        builder.HasMany(c => c.ComplianceDocuments)
            .WithOne()
            .HasForeignKey(d => d.CarrierId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.ComplianceDocuments)
            .HasField("_complianceDocuments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.MapXminRowVersion();
        builder.Ignore(c => c.DomainEvents);
    }
}

internal sealed class ComplianceDocumentConfiguration : IEntityTypeConfiguration<ComplianceDocument>
{
    public void Configure(EntityTypeBuilder<ComplianceDocument> builder)
    {
        builder.ToTable("compliance_documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DocumentType).HasConversion<string>().HasMaxLength(48);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(d => d.BlobUri).HasMaxLength(1024).IsRequired();
        builder.Property(d => d.OriginalFileName).HasMaxLength(400).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(160).IsRequired();
    }
}

internal sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.ContactEmail).HasMaxLength(256).IsRequired();
        builder.Property(l => l.ContactName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Phone).HasMaxLength(40).IsRequired();
        builder.Property(l => l.VerticalSlug).HasMaxLength(120).IsRequired();
        builder.Property(l => l.CargoDetails).HasMaxLength(2000);
        builder.Property(l => l.ServiceType).HasConversion<string>().HasMaxLength(32);
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.VerticalSlug);
        builder.Ignore(l => l.DomainEvents);
    }
}

internal sealed class LoadConfiguration : IEntityTypeConfiguration<Load>
{
    public void Configure(EntityTypeBuilder<Load> builder)
    {
        builder.ToTable("loads");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Reference).HasMaxLength(64).IsRequired();
        builder.HasIndex(l => l.Reference).IsUnique();
        builder.Property(l => l.ServiceType).HasConversion<string>().HasMaxLength(32);
        builder.Property(l => l.OriginZip).HasMaxLength(16).IsRequired();
        builder.Property(l => l.DestinationZip).HasMaxLength(16).IsRequired();
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(l => l.Status);
        builder.MapXminRowVersion();
        builder.Ignore(l => l.DomainEvents);
    }
}

internal sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(120).IsRequired();
        builder.Property(a => a.EntityName).HasMaxLength(160).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(120);
        builder.Property(a => a.UserId).HasMaxLength(120);
        builder.Property(a => a.TraceId).HasMaxLength(64);
        builder.Property(a => a.DataJson).HasColumnType("jsonb");
        builder.HasIndex(a => a.TimestampUtc);
    }
}

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records");
        builder.HasKey(r => r.Key);
        builder.Property(r => r.Key).HasMaxLength(128);
        builder.Property(r => r.RequestPath).HasMaxLength(400).IsRequired();
        builder.Property(r => r.ResponseBody).HasColumnType("jsonb");
        builder.HasIndex(r => r.CreatedAtUtc);
    }
}
