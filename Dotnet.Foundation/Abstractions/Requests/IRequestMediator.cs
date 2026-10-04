namespace Dotnet.Foundation.Abstractions.Requests;

/// <summary>
/// Defines a mediator for sending requests to their corresponding handlers.
/// </summary>
public interface IRequestMediator
{
    /// <summary>
    /// Asynchronously sends a request of type <see cref = "IRequest{TResult}" /> to the corresponding handler of type <see cref = "IRequestHandler{TRequest,TResult}" /> through the corresponding behaviors of type <see cref = "IRequestBehavior{TRequest,TResult}" />.
    /// </summary>
    /// <returns>A value of type <typeparamref name = "TResult" /> representing the result of the request.</returns>
    /// <exception cref = "InvalidOperationException">No request handler is registered for the request type.</exception>
    public Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously sends a request of type <see cref = "IRequest" /> to the corresponding handler of type <see cref = "IRequestHandler{TRequest}" /> through the corresponding behaviors of type <see cref = "IRequestBehavior{TRequest}" />.
    /// </summary>
    /// <exception cref = "InvalidOperationException">No request handler is registered for the request type.</exception>
    public Task SendAsync(IRequest request, CancellationToken cancellationToken = default);
}
