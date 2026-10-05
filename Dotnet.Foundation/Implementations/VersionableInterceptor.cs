using Dotnet.Foundation.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dotnet.Foundation.Implementations;

/// <summary>
/// Represents the interceptor changing the version of the added and modified entities implementing <see cref = "IVersionable" /> when saving changes.
/// </summary>
public sealed class VersionableInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateVersionableEntities(eventData.Context);

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateVersionableEntities(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private static void UpdateVersionableEntities(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var versionableEntries = dbContext.ChangeTracker.Entries<IVersionable>()
                                          .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
                                          .ToList();

        foreach (var versionableEntry in versionableEntries)
        {
            versionableEntry.Property(entity => entity.Version).CurrentValue = Guid.NewGuid();
        }
    }
}
