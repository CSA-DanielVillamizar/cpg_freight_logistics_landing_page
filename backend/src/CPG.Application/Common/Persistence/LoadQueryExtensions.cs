using CPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPG.Application.Common.Persistence;

/// <summary>Query shaping helpers for <see cref="Load"/> command handlers.</summary>
public static class LoadQueryExtensions
{
    /// <summary>
    /// A load addressed by its id inside a command handler. Bypasses the synthetic-data
    /// (<c>CPG-E2E-</c>) global query filter so end-to-end fixtures stay fully operable, while
    /// still hiding soft-deleted rows — a deleted load must not be acceptable or deliverable.
    /// </summary>
    public static IQueryable<Load> OperableById(this IQueryable<Load> loads)
        => loads.IgnoreQueryFilters().Where(load => !load.IsDeleted);
}
