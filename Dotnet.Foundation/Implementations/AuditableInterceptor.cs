using Dotnet.Foundation.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dotnet.Foundation.Implementations;

/// <summary>
/// Represents the interceptor setting the creation and update timestamps of the entities implementing <see cref = "IAuditable" /> when saving changes.
/// </summary>
public sealed class AuditableInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public AuditableInterceptor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private void UpdateAuditableEntities(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var now = _timeProvider.GetUtcNow();

        var auditableEntries = dbContext.ChangeTracker.Entries<IAuditable>()
                                        .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
                                        .ToList();

        foreach (var auditableEntry in auditableEntries)
        {
            if (auditableEntry.State is EntityState.Added)
            {
                auditableEntry.Property(entity => entity.CreatedAt).CurrentValue = now;
            }
            else
            {
                auditableEntry.Property(entity => entity.UpdatedAt).CurrentValue = now;
            }
        }
    }
}
