namespace Dotnet.Foundation.Abstractions.Requests;

/// <summary>
/// Defines a behavior running around the handler for requests of type <typeparamref name = "TRequest" /> with a result of type <typeparamref name = "TResult" />.
/// </summary>
public interface IRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    /// <summary>
    /// Asynchronously handles a request of type <typeparamref name = "TRequest" /> around the next behavior or the handler.
    /// </summary>
    /// <returns>A value of type <typeparamref name = "TResult" /> representing the result of the request.</returns>
    public Task<TResult> HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a behavior running around the handler for requests of type <typeparamref name = "TRequest" /> without a result.
/// </summary>
public interface IRequestBehavior<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Asynchronously handles a request of type <typeparamref name = "TRequest" /> around the next behavior or the handler.
    /// </summary>
    public Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken = default);
}
