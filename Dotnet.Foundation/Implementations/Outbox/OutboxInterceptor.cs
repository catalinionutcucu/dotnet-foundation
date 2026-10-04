using Dotnet.Foundation.Models.Outbox;
using Dotnet.Foundation.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace Dotnet.Foundation.Implementations.Outbox;

/// <summary>
/// Represents the interceptor storing the domain events raised by the entities of type <see cref = "Entity" /> as outbox messages of type <see cref = "OutboxMessage" /> when saving changes.
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public OutboxInterceptor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        InsertOutboxMessages(eventData.Context);

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        InsertOutboxMessages(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private void InsertOutboxMessages(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var now = _timeProvider.GetUtcNow();

        var entitiesWithDomainEvents = dbContext.ChangeTracker.Entries<Entity>()
                                                .Select(entry => entry.Entity)
                                                .Where(entity => entity.DomainEvents.Any())
                                                .ToList();

        var outboxMessages = entitiesWithDomainEvents.SelectMany(entity => entity.DomainEvents)
                                                     .Select(domainEvent => new OutboxMessage($"{domainEvent.GetType().FullName}, {domainEvent.GetType().Assembly.GetName().Name}", JsonSerializer.Serialize(domainEvent, domainEvent.GetType()), now))
                                                     .ToList();

        foreach (var entityWithDomainEvents in entitiesWithDomainEvents)
        {
            entityWithDomainEvents.ClearDomainEvents();
        }

        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
