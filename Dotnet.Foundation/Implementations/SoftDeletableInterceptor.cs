using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.SoftDeletable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dotnet.Foundation.Implementations;

/// <summary>
/// Represents the interceptor setting the deletion timestamp of the entities implementing <see cref = "ISoftDeletable" />, and the user deleting the entities implementing <see cref = "IUserSoftDeletable" />, instead of removing them when saving changes.
/// </summary>
public sealed class SoftDeletableInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    private readonly ICurrentUser _currentUser;

    public SoftDeletableInterceptor(TimeProvider timeProvider, ICurrentUser currentUser)
    {
        _timeProvider = timeProvider;
        _currentUser = currentUser;
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

        var currentUserId = _currentUser.Id;

        var softDeletableEntries = dbContext.ChangeTracker.Entries<ISoftDeletable>()
                                            .Where(entry => entry.State is EntityState.Deleted)
                                            .ToList();

        foreach (var softDeletableEntry in softDeletableEntries)
        {
            softDeletableEntry.State = EntityState.Unchanged;
            softDeletableEntry.Property(entity => entity.DeletedAt).CurrentValue = now;

            if (softDeletableEntry.Entity is IUserSoftDeletable)
            {
                softDeletableEntry.Property(nameof(IUserSoftDeletable.DeletedBy)).CurrentValue = currentUserId;
            }
        }
    }
}
