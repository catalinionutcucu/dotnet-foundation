using Dotnet.Foundation.Abstractions.DomainEvents;
using Dotnet.Foundation.Models.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Dotnet.Foundation.Implementations.Outbox;

/// <summary>
/// Represents the background service periodically publishing the unprocessed outbox messages of type <see cref = "OutboxMessage" /> stored in the database context of type <typeparamref name = "TDbContext" /> to their handlers of type <see cref = "IDomainEventHandler{TDomainEvent}" />.
/// </summary>
public sealed class OutboxProcessor<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private static readonly ConcurrentDictionary<Type, DomainEventHandlersResolver> DomainEventHandlersResolvers = new();

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly TimeProvider _timeProvider;

    private readonly OutboxOptions _outboxOptions;

    private readonly ILogger<OutboxProcessor<TDbContext>> _logger;

    public OutboxProcessor(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider, IOptions<OutboxOptions> outboxOptions, ILogger<OutboxProcessor<TDbContext>> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _timeProvider = timeProvider;
        _outboxOptions = outboxOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var periodicTimer = new PeriodicTimer(_outboxOptions.Interval, _timeProvider);

        do
        {
            try
            {
                int outboxMessageCount;

                do
                {
                    outboxMessageCount = await ProcessOutboxMessagesAsync(stoppingToken).ConfigureAwait(false);
                }
                while (outboxMessageCount == _outboxOptions.BatchSize);

                await DeleteProcessedOutboxMessagesAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(exception, "An exception occurred while processing the outbox messages.");
            }
        }
        while (await periodicTimer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }

    private delegate IReadOnlyList<(string Name, Func<CancellationToken, Task> HandleAsync)> DomainEventHandlersResolver(IServiceProvider serviceProvider, IDomainEvent domainEvent);

    private async Task<int> ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();

        var dbContext = serviceScope.ServiceProvider.GetRequiredService<TDbContext>();

        var now = _timeProvider.GetUtcNow();

        var maxAttempts = _outboxOptions.MaxAttempts;

        var outboxMessages = await dbContext.Set<OutboxMessage>()
                                            .Where(outboxMessage => outboxMessage.ProcessedAt == null && outboxMessage.Attempts < maxAttempts)
                                            .Where(outboxMessage => outboxMessage.NextAttemptAt == null || outboxMessage.NextAttemptAt <= now)
                                            .OrderBy(outboxMessage => outboxMessage.OccurredAt)
                                            .Take(_outboxOptions.BatchSize)
                                            .ToListAsync(cancellationToken)
                                            .ConfigureAwait(false);

        foreach (var outboxMessage in outboxMessages)
        {
            if (await TryClaimOutboxMessageAsync(dbContext, outboxMessage, cancellationToken).ConfigureAwait(false))
            {
                await ProcessOutboxMessageAsync(dbContext, outboxMessage, cancellationToken).ConfigureAwait(false);
            }
        }

        return outboxMessages.Count;
    }

    private async Task<bool> TryClaimOutboxMessageAsync(TDbContext dbContext, OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        outboxMessage.Claim(_timeProvider.GetUtcNow() + _outboxOptions.ClaimDuration);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.Entry(outboxMessage).State = EntityState.Detached;

            return false;
        }
    }

    private async Task ProcessOutboxMessageAsync(TDbContext dbContext, OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        try
        {
            var domainEventType = Type.GetType(outboxMessage.Type, true)!;

            var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(outboxMessage.Content, domainEventType)!;

            using var domainEventServiceScope = _serviceScopeFactory.CreateScope();

            var domainEventHandlersResolver = DomainEventHandlersResolvers.GetOrAdd(domainEventType, type => CreateDomainEventHandlersResolver(type));

            var domainEventHandlers = domainEventHandlersResolver(domainEventServiceScope.ServiceProvider, domainEvent);

            foreach (var domainEventHandler in domainEventHandlers.Where(domainEventHandler => !outboxMessage.CompletedHandlers.Contains(domainEventHandler.Name)))
            {
                await domainEventHandler.HandleAsync(cancellationToken).ConfigureAwait(false);

                outboxMessage.CompleteHandler(domainEventHandler.Name);
            }

            outboxMessage.MarkAsProcessed(_timeProvider.GetUtcNow());
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            if (outboxMessage.Attempts < _outboxOptions.MaxAttempts)
            {
                _logger.LogWarning(exception, "An exception occurred while processing the outbox message '{OutboxMessageId}' on attempt {Attempt} of {MaxAttempts}.", outboxMessage.Id, outboxMessage.Attempts, _outboxOptions.MaxAttempts);
            }
            else
            {
                _logger.LogError(exception, "An exception occurred while processing the outbox message '{OutboxMessageId}' on the last attempt of {MaxAttempts}.", outboxMessage.Id, _outboxOptions.MaxAttempts);
            }

            var retryDelay = TimeSpan.FromTicks((long)Math.Min(_outboxOptions.RetryDelay.Ticks * Math.Pow(2, outboxMessage.Attempts - 1), _outboxOptions.MaxRetryDelay.Ticks));

            outboxMessage.MarkAsFailed(exception.ToString(), _timeProvider.GetUtcNow() + retryDelay);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("The claim on the outbox message '{OutboxMessageId}' expired before the message was processed.", outboxMessage.Id);

            dbContext.Entry(outboxMessage).State = EntityState.Detached;
        }
    }

    private async Task DeleteProcessedOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();

        var dbContext = serviceScope.ServiceProvider.GetRequiredService<TDbContext>();

        var processedBefore = _timeProvider.GetUtcNow() - _outboxOptions.RetentionPeriod;

        await dbContext.Set<OutboxMessage>()
                       .Where(outboxMessage => outboxMessage.ProcessedAt != null && outboxMessage.ProcessedAt < processedBefore)
                       .ExecuteDeleteAsync(cancellationToken)
                       .ConfigureAwait(false);
    }

    private static DomainEventHandlersResolver CreateDomainEventHandlersResolver(Type domainEventType)
    {
        return typeof(OutboxProcessor<TDbContext>).GetMethod(nameof(ResolveDomainEventHandlers), BindingFlags.NonPublic | BindingFlags.Static)!
                                                  .MakeGenericMethod(domainEventType)
                                                  .CreateDelegate<DomainEventHandlersResolver>();
    }

    private static IReadOnlyList<(string Name, Func<CancellationToken, Task> HandleAsync)> ResolveDomainEventHandlers<TDomainEvent>(IServiceProvider serviceProvider, IDomainEvent domainEvent)
        where TDomainEvent : IDomainEvent
    {
        return serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>()
                              .Select(domainEventHandler => (domainEventHandler.GetType().FullName!, (Func<CancellationToken, Task>)(cancellationToken => domainEventHandler.HandleAsync((TDomainEvent)domainEvent, cancellationToken))))
                              .ToList();
    }
}
