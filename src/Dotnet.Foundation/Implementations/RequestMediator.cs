using Dotnet.Foundation.Abstractions.Requests;

namespace Dotnet.Foundation.Implementations;

public sealed class RequestMediator : IRequestMediator
{
    private readonly IServiceProvider _serviceProvider;

    public RequestMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResult));

        dynamic requestHandler = _serviceProvider.GetService(requestHandlerType);

        if (requestHandler is null)
        {
            throw new InvalidOperationException($"No request handler found for request type '{requestType.FullName}'.");
        }

        Task<TResult> requestHandlerTask = requestHandler.HandleAsync((dynamic)request, cancellationToken);

        return await requestHandlerTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var requestHandlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        dynamic requestHandler = _serviceProvider.GetService(requestHandlerType);

        if (requestHandler is null)
        {
            throw new InvalidOperationException($"No request handler found for request type '{requestType.FullName}'.");
        }

        Task requestHandlerTask = requestHandler.HandleAsync((dynamic)request, cancellationToken);

        await requestHandlerTask.ConfigureAwait(false);
    }
}
