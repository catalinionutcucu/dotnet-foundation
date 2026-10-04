using Dotnet.Foundation.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dotnet.Foundation.Implementations;

/// <summary>
/// Represents the interceptor setting the deletion timestamp of the entities implementing <see cref = "ISoftDeletable" /> instead of removing them when saving changes.
/// </summary>
public sealed class SoftDeletableInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public SoftDeletableInterceptor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateSoftDeletableEntities(eventData.Context);

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateSoftDeletableEntities(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private void UpdateSoftDeletableEntities(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var now = _timeProvider.GetUtcNow();

        var softDeletableEntries = dbContext.ChangeTracker.Entries<ISoftDeletable>()
                                            .Where(entry => entry.State is EntityState.Deleted)
                                            .ToList();

        foreach (var softDeletableEntry in softDeletableEntries)
        {
            softDeletableEntry.State = EntityState.Unchanged;
            softDeletableEntry.Property(entity => entity.DeletedAt).CurrentValue = now;
        }
    }
}
