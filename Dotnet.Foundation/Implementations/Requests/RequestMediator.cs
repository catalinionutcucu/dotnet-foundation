using Dotnet.Foundation.Abstractions.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace Dotnet.Foundation.Implementations.Requests;

/// <summary>
/// Represents the request mediator implementing <see cref = "IRequestMediator" />, resolving the corresponding handlers and behaviors from the service provider.
/// </summary>
public sealed class RequestMediator : IRequestMediator
{
    private static readonly ConcurrentDictionary<Type, Delegate> RequestPipelines = new();

    private static readonly ConcurrentDictionary<Type, RequestPipeline> RequestPipelinesWithoutResult = new();

    private readonly IServiceProvider _serviceProvider;

    public RequestMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestPipeline = (RequestPipeline<TResult>)RequestPipelines.GetOrAdd(request.GetType(), requestType => CreateRequestPipeline<TResult>(requestType));

        return await requestPipeline(_serviceProvider, request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestPipeline = RequestPipelinesWithoutResult.GetOrAdd(request.GetType(), requestType => CreateRequestPipeline(requestType));

        await requestPipeline(_serviceProvider, request, cancellationToken).ConfigureAwait(false);
    }

    private delegate Task<TResult> RequestPipeline<TResult>(IServiceProvider serviceProvider, IRequest<TResult> request, CancellationToken cancellationToken);

    private delegate Task RequestPipeline(IServiceProvider serviceProvider, IRequest request, CancellationToken cancellationToken);

    private static RequestPipeline<TResult> CreateRequestPipeline<TResult>(Type requestType)
    {
        return typeof(RequestMediator).GetMethod(nameof(RunRequestPipelineAsync), BindingFlags.NonPublic | BindingFlags.Static)!
                                      .MakeGenericMethod(requestType, typeof(TResult))
                                      .CreateDelegate<RequestPipeline<TResult>>();
    }

    private static RequestPipeline CreateRequestPipeline(Type requestType)
    {
        return typeof(RequestMediator).GetMethod(nameof(RunRequestPipelineWithoutResultAsync), BindingFlags.NonPublic | BindingFlags.Static)!
                                      .MakeGenericMethod(requestType)
                                      .CreateDelegate<RequestPipeline>();
    }

    private static Task<TResult> RunRequestPipelineAsync<TRequest, TResult>(IServiceProvider serviceProvider, IRequest<TResult> request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResult>
    {
        var requestHandler = serviceProvider.GetService<IRequestHandler<TRequest, TResult>>();

        if (requestHandler is null)
        {
            throw new InvalidOperationException($"No request handler found for request type '{typeof(TRequest).FullName}'.");
        }

        var requestBehaviors = serviceProvider.GetServices<IRequestBehavior<TRequest, TResult>>();

        var handleAsync = () => requestHandler.HandleAsync((TRequest)request, cancellationToken);

        foreach (var requestBehavior in requestBehaviors.Reverse())
        {
            var nextHandleAsync = handleAsync;

            handleAsync = () => requestBehavior.HandleAsync((TRequest)request, nextHandleAsync, cancellationToken);
        }

        return handleAsync();
    }

    private static Task RunRequestPipelineWithoutResultAsync<TRequest>(IServiceProvider serviceProvider, IRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        var requestHandler = serviceProvider.GetService<IRequestHandler<TRequest>>();

        if (requestHandler is null)
        {
            throw new InvalidOperationException($"No request handler found for request type '{typeof(TRequest).FullName}'.");
        }

        var requestBehaviors = serviceProvider.GetServices<IRequestBehavior<TRequest>>();

        var handleAsync = () => requestHandler.HandleAsync((TRequest)request, cancellationToken);

        foreach (var requestBehavior in requestBehaviors.Reverse())
        {
            var nextHandleAsync = handleAsync;

            handleAsync = () => requestBehavior.HandleAsync((TRequest)request, nextHandleAsync, cancellationToken);
        }

        return handleAsync();
    }
}
