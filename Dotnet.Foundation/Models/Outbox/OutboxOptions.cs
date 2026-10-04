namespace Dotnet.Foundation.Models.Outbox;

/// <summary>
/// Represents the options for processing the outbox messages.
/// </summary>
public sealed class OutboxOptions
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(10);

    public int BatchSize { get; set; } = 20;

    public int MaxAttempts { get; set; } = 5;

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromHours(1);

    public TimeSpan ClaimDuration { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(7);
}
