namespace Dotnet.Foundation.Models.Outbox;

/// <summary>
/// Represents a domain event stored in the outbox with the changes raising it, to be published to its handlers after the changes are saved.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Content { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public DateTimeOffset? NextAttemptAt { get; private set; }

    public int Attempts { get; private set; }

    public string? Error { get; private set; }

    public IReadOnlyList<string> CompletedHandlers { get; private set; } = [ ];

    public Guid Version { get; private set; }

    public OutboxMessage(string type, string content, DateTimeOffset occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Id = Guid.CreateVersion7();
        Type = type;
        Content = content;
        OccurredAt = occurredAt;
        Version = Guid.NewGuid();
    }

    /// <summary>
    /// Claims the outbox message for processing until the specified time, counting the attempt.
    /// </summary>
    public void Claim(DateTimeOffset claimedUntil)
    {
        Attempts++;
        NextAttemptAt = claimedUntil;
        Version = Guid.NewGuid();
    }

    /// <summary>
    /// Marks the handler with the specified name as completed, so it is skipped when the outbox message is attempted again.
    /// </summary>
    public void CompleteHandler(string handlerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(handlerName);

        CompletedHandlers = [ ..CompletedHandlers, handlerName ];
    }

    /// <summary>
    /// Marks the outbox message as processed at the specified time.
    /// </summary>
    public void MarkAsProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
        NextAttemptAt = null;
        Error = null;
        Version = Guid.NewGuid();
    }

    /// <summary>
    /// Marks the outbox message as failed with the specified error, to be attempted again at the specified time.
    /// </summary>
    public void MarkAsFailed(string error, DateTimeOffset nextAttemptAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        NextAttemptAt = nextAttemptAt;
        Error = error;
        Version = Guid.NewGuid();
    }
}
