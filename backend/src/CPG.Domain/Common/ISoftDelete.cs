namespace CPG.Domain.Common;

/// <summary>
/// Marks an entity that is never physically removed. A global EF Core query filter hides rows
/// where <see cref="IsDeleted"/> is <c>true</c> from every default query; administrative
/// audit reads opt back in with <c>IgnoreQueryFilters()</c>.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
