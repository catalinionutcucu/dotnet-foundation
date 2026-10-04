using Dotnet.Foundation.Abstractions.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Dotnet.Foundation.Implementations.Requests;

/// <summary>
/// Represents the request mediator implementing <see cref = "IRequestMediator" />, resolving the corresponding handlers and behaviors from the service provider.
/// </summary>
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

        var requestBehaviorType = typeof(IRequestBehavior<,>).MakeGenericType(requestType, typeof(TResult));

        var requestBehaviors = _serviceProvider.GetServices(requestBehaviorType);

        var handleAsync = () => (Task<TResult>)InvokeHandleAsync(requestHandlerType, requestHandler, [ request, cancellationToken ]);

        foreach (var requestBehavior in requestBehaviors.Reverse())
        {
            var nextHandleAsync = handleAsync;

            handleAsync = () => (Task<TResult>)InvokeHandleAsync(requestBehaviorType, requestBehavior!, [ request, nextHandleAsync, cancellationToken ]);
        }

        return await handleAsync().ConfigureAwait(false);
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

        var requestBehaviorType = typeof(IRequestBehavior<>).MakeGenericType(requestType);

        var requestBehaviors = _serviceProvider.GetServices(requestBehaviorType);

        var handleAsync = () => (Task)InvokeHandleAsync(requestHandlerType, requestHandler, [ request, cancellationToken ]);

        foreach (var requestBehavior in requestBehaviors.Reverse())
        {
            var nextHandleAsync = handleAsync;

            handleAsync = () => (Task)InvokeHandleAsync(requestBehaviorType, requestBehavior!, [ request, nextHandleAsync, cancellationToken ]);
        }

        await handleAsync().ConfigureAwait(false);
    }

    private static object InvokeHandleAsync(Type serviceType, object service, object?[] arguments)
    {
        var handleAsyncMethod = serviceType.GetMethod(nameof(IRequestHandler<>.HandleAsync))!;

        return handleAsyncMethod.Invoke(service, BindingFlags.DoNotWrapExceptions, null, arguments, null)!;
    }
}
