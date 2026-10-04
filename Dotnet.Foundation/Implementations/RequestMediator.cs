using Dotnet.Foundation.Abstractions.Requests;
using System.Reflection;

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

        var requestHandler = _serviceProvider.GetService(requestHandlerType);

        if (requestHandler is null)
        {
            throw new InvalidOperationException($"No request handler found for request type '{requestType.FullName}'.");
        }

        var requestHandlerTask = (Task<TResult>)InvokeRequestHandler(requestHandlerType, requestHandler, request, cancellationToken);

        return await requestHandlerTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        var requestHandlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        var requestHandler = _serviceProvider.GetService(requestHandlerType);

        if (requestHandler is null)
        {
            throw new InvalidOperationException($"No request handler found for request type '{requestType.FullName}'.");
        }

        var requestHandlerTask = (Task)InvokeRequestHandler(requestHandlerType, requestHandler, request, cancellationToken);

        await requestHandlerTask.ConfigureAwait(false);
    }

    private static object InvokeRequestHandler(Type requestHandlerType, object requestHandler, object request, CancellationToken cancellationToken)
    {
        var handleAsyncMethod = requestHandlerType.GetMethod(nameof(IRequestHandler<>.HandleAsync))!;

        return handleAsyncMethod.Invoke(requestHandler, BindingFlags.DoNotWrapExceptions, null, [ request, cancellationToken ], null)!;
    }
}
