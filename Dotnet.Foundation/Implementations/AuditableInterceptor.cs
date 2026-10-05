using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.Auditable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dotnet.Foundation.Implementations;

/// <summary>
/// Represents the interceptor setting the creation and update timestamps of the entities implementing <see cref = "IAuditable" />, and the users creating and updating the entities implementing <see cref = "IUserAuditable" />, when saving changes.
/// </summary>
public sealed class AuditableInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    private readonly ICurrentUser _currentUser;

    public AuditableInterceptor(TimeProvider timeProvider, ICurrentUser currentUser)
    {
        _timeProvider = timeProvider;
        _currentUser = currentUser;
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

        var currentUserId = _currentUser.Id;

        var auditableEntries = dbContext.ChangeTracker.Entries<IAuditable>()
                                        .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
                                        .ToList();

        foreach (var auditableEntry in auditableEntries)
        {
            if (auditableEntry.State is EntityState.Added)
            {
                auditableEntry.Property(entity => entity.CreatedAt).CurrentValue = now;

                if (auditableEntry.Entity is IUserAuditable)
                {
                    auditableEntry.Property(nameof(IUserAuditable.CreatedBy)).CurrentValue = currentUserId;
                }
            }
            else
            {
                auditableEntry.Property(entity => entity.UpdatedAt).CurrentValue = now;

                if (auditableEntry.Entity is IUserAuditable)
                {
                    auditableEntry.Property(nameof(IUserAuditable.UpdatedBy)).CurrentValue = currentUserId;
                }
            }
        }
    }
}
