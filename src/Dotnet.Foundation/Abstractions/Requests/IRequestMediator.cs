namespace Dotnet.Foundation.Abstractions.Requests;

public interface IRequestMediator
{
    /// <summary>
    /// Sends a request of type <see cref = "IRequest{TResult}" /> to the corresponding handler of type <see cref = "IRequestHandler{TRequest,TResult}" />.
    /// </summary>
    /// <returns>A task that produces a value of type <typeparamref name = "TResult" /> representing the result of the request.</returns>
    public Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request of type <see cref = "IRequest" /> to the corresponding handler of type <see cref = "IRequestHandler{TRequest}" />.
    /// </summary>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendAsync(IRequest request, CancellationToken cancellationToken = default);
}
