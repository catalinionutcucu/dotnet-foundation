using Dotnet.Foundation.Models;

namespace Dotnet.Foundation.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Matches a <see cref = "Result{TValue,TError}" /> instance to a value of type <typeparamref name = "TMap" /> based on the result state.
    /// </summary>
    /// <returns>A value of type <typeparamref name = "TMap" />.</returns>
    public static TMap Match<TMap, TValue, TError>(this Result<TValue, TError> result, Func<TValue, TMap> success, Func<TError, TMap> failure)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return result.IsSuccess ? success(result.Value) : failure(result.Error);
    }

    /// <summary>
    /// Matches a <see cref = "Result{TError}" /> instance to a value of type <typeparamref name = "TMap" /> based on the result state.
    /// </summary>
    /// <returns>A value of type <typeparamref name = "TMap" />.</returns>
    public static TMap Match<TMap, TError>(this Result<TError> result, Func<TMap> success, Func<TError, TMap> failure)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return result.IsSuccess ? success() : failure(result.Error);
    }
}
