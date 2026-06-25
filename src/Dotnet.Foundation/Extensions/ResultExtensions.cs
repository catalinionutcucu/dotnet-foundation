using Dotnet.Foundation.Models;

namespace Dotnet.Foundation.Extensions;

public static class ResultExtensions
{
    extension<TValue, TError>(Result<TValue, TError> result)
    {
        /// <summary>
        /// Matches a <see cref = "Result{TValue,TError}" /> instance to a value of type <typeparamref name = "TMap" /> based on the result state.
        /// </summary>
        /// <returns>A value of type <typeparamref name = "TMap" />.</returns>
        public TMap Match<TMap>(Func<TValue, TMap> success, Func<TError, TMap> failure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(success);
            ArgumentNullException.ThrowIfNull(failure);

            return result.IsSuccess ? success(result.Value) : failure(result.Error);
        }
    }

    extension<TError>(Result<TError> result)
    {
        /// <summary>
        /// Matches a <see cref = "Result{TError}" /> instance to a value of type <typeparamref name = "TMap" /> based on the result state.
        /// </summary>
        /// <returns>A value of type <typeparamref name = "TMap" />.</returns>
        public TMap Match<TMap>(Func<TMap> success, Func<TError, TMap> failure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(success);
            ArgumentNullException.ThrowIfNull(failure);

            return result.IsSuccess ? success() : failure(result.Error);
        }
    }
}
