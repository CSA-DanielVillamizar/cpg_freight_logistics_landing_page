using CPG.Application.Common.Interfaces;
using CPG.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CPG.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Stamps <see cref="IAuditableEntity"/> create/update columns automatically on every
/// <c>SaveChanges</c> (SPEC.md section 1 - persistence concerns stay out of handlers).
/// </summary>
public sealed class AuditableEntityInterceptor(
    ICurrentUser currentUser,
    IDateTimeProvider dateTime) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyStamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyStamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyStamps(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var actor = currentUser.Email ?? currentUser.UserId?.ToString() ?? "system";
        var now = dateTime.UtcNow;

        foreach (EntityEntry<IAuditableEntity> entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedBy = actor;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.LastModifiedAtUtc = now;
                entry.Entity.LastModifiedBy = actor;
            }
        }
    }
}
