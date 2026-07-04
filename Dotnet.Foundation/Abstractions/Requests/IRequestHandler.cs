namespace Dotnet.Foundation.Abstractions.Requests;

public interface IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    /// <summary>
    /// Handles a request of type <typeparamref name = "TRequest" />.
    /// </summary>
    /// <returns>A value of type <typeparamref name = "TResult" /> representing the result of the request.</returns>
    public Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Handles a request of type <typeparamref name = "TRequest" />.
    /// </summary>
    public Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
