using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Models;
using Microsoft.Extensions.Logging;

namespace Dotnet.Foundation.Implementations.Requests;

/// <summary>
/// Represents the request behavior implementing <see cref = "IRequestBehavior{TRequest,TResult}" />, logging the duration of the requests with their failure results and exceptions.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResult> : IRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly TimeProvider _timeProvider;

    private readonly ILogger<LoggingBehavior<TRequest, TResult>> _logger;

    public LoggingBehavior(TimeProvider timeProvider, ILogger<LoggingBehavior<TRequest, TResult>> logger)
    {
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResult> HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var startTimestamp = _timeProvider.GetTimestamp();

        try
        {
            var result = await next().ConfigureAwait(false);

            var elapsedMilliseconds = _timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds;

            if (result is IFailureResult<TResult, RequestError> { IsFailure: true } failureResult)
            {
                _logger.LogWarning("Handled the request '{RequestName}' in {ElapsedMilliseconds:0.0} ms with a failure of type '{ErrorType}' and code '{ErrorCode}'.", typeof(TRequest).Name, elapsedMilliseconds, failureResult.Error.Type, failureResult.Error.Code);
            }
            else
            {
                _logger.LogInformation("Handled the request '{RequestName}' in {ElapsedMilliseconds:0.0} ms.", typeof(TRequest).Name, elapsedMilliseconds);
            }

            return result;
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(exception, "An exception occurred while handling the request '{RequestName}' after {ElapsedMilliseconds:0.0} ms.", typeof(TRequest).Name, _timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds);

            throw;
        }
    }
}

/// <summary>
/// Represents the request behavior implementing <see cref = "IRequestBehavior{TRequest}" />, logging the duration of the requests without a result with their exceptions.
/// </summary>
public sealed class LoggingBehavior<TRequest> : IRequestBehavior<TRequest>
    where TRequest : IRequest
{
    private readonly TimeProvider _timeProvider;

    private readonly ILogger<LoggingBehavior<TRequest>> _logger;

    public LoggingBehavior(TimeProvider timeProvider, ILogger<LoggingBehavior<TRequest>> logger)
    {
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var startTimestamp = _timeProvider.GetTimestamp();

        try
        {
            await next().ConfigureAwait(false);

            _logger.LogInformation("Handled the request '{RequestName}' in {ElapsedMilliseconds:0.0} ms.", typeof(TRequest).Name, _timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(exception, "An exception occurred while handling the request '{RequestName}' after {ElapsedMilliseconds:0.0} ms.", typeof(TRequest).Name, _timeProvider.GetElapsedTime(startTimestamp).TotalMilliseconds);

            throw;
        }
    }
}
