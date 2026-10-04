namespace Dotnet.Foundation.Abstractions.Requests;

/// <summary>
/// Marks a request with a result of type <typeparamref name = "TResult" /> handled by the corresponding handler of type <see cref = "IRequestHandler{TRequest,TResult}" />.
/// </summary>
public interface IRequest<TResult>;

/// <summary>
/// Marks a request without a result handled by the corresponding handler of type <see cref = "IRequestHandler{TRequest}" />.
/// </summary>
public interface IRequest;
