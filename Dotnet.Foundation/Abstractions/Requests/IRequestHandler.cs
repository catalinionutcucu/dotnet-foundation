namespace Dotnet.Foundation.Abstractions.Requests;

/// <summary>
/// Defines a handler for requests of type <typeparamref name = "TRequest" /> with a result of type <typeparamref name = "TResult" />.
/// </summary>
public interface IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    /// <summary>
    /// Asynchronously handles a request of type <typeparamref name = "TRequest" />.
    /// </summary>
    /// <returns>A value of type <typeparamref name = "TResult" /> representing the result of the request.</returns>
    public Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for requests of type <typeparamref name = "TRequest" /> without a result.
/// </summary>
public interface IRequestHandler<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Asynchronously handles a request of type <typeparamref name = "TRequest" />.
    /// </summary>
    public Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
