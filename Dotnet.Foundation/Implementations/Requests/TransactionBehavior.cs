using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.Foundation.Implementations.Requests;

/// <summary>
/// Represents the request behavior implementing <see cref = "IRequestBehavior{TRequest,TResult}" />, handling the requests implementing <see cref = "ITransactionalRequest" /> in a database transaction, committed when the result is not a failure result and rolled back otherwise.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResult> : IRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>, ITransactionalRequest
{
    private readonly DbContext _dbContext;

    public TransactionBehavior(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<TResult> HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await next().ConfigureAwait(false);
        }

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
                                      {
                                          await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                                          var result = await next().ConfigureAwait(false);

                                          if (result is IFailureResult<TResult, RequestError> { IsFailure: true })
                                          {
                                              await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                                          }
                                          else
                                          {
                                              await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                                          }

                                          return result;
                                      })
                                      .ConfigureAwait(false);
    }
}

/// <summary>
/// Represents the request behavior implementing <see cref = "IRequestBehavior{TRequest}" />, handling the requests implementing <see cref = "ITransactionalRequest" /> in a database transaction, committed when the request succeeds and rolled back when it throws.
/// </summary>
public sealed class TransactionBehavior<TRequest> : IRequestBehavior<TRequest>
    where TRequest : IRequest, ITransactionalRequest
{
    private readonly DbContext _dbContext;

    public TransactionBehavior(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        if (_dbContext.Database.CurrentTransaction is not null)
        {
            await next().ConfigureAwait(false);

            return;
        }

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
                               {
                                   await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                                   await next().ConfigureAwait(false);

                                   await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                               })
                               .ConfigureAwait(false);
    }
}
