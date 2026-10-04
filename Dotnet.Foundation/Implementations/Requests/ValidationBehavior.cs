using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Models;
using FluentValidation;

namespace Dotnet.Foundation.Implementations.Requests;

/// <summary>
/// Represents the request behavior implementing <see cref = "IRequestBehavior{TRequest,TResult}" />, validating the requests with the validators implementing <see cref = "IValidator{T}" /> and returning a failure result with a <see cref = "RequestError" /> instead of calling the next behavior or the handler when the validation fails.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResult> : IRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : IFailureResult<TResult, RequestError>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <inheritdoc />
    public async Task<TResult> HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var issues = new List<string>();

        foreach (var validator in _validators)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken)
                                                  .ConfigureAwait(false);

            issues.AddRange(validationResult.Errors.Select(validationFailure => validationFailure.ErrorMessage));
        }

        if (issues.Any())
        {
            return TResult.Failure(RequestError.RequestInvalid("request.invalid", issues));
        }

        return await next().ConfigureAwait(false);
    }
}
