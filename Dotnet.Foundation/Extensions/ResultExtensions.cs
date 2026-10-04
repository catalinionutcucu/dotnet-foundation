using Dotnet.Foundation.Models;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for matching, mapping, binding and ensuring <see cref = "Result{TValue,TError}" /> and <see cref = "Result{TError}" /> instances.
/// </summary>
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

        /// <summary>
        /// Maps a <see cref = "Result{TValue,TError}" /> instance to a <see cref = "Result{TValue,TError}" /> instance with a value of type <typeparamref name = "TMap" /> if the result is a success result.
        /// </summary>
        /// <returns>A <see cref = "Result{TValue,TError}" /> instance with the mapped value, or with the error of the result if the result is a failure result.</returns>
        public Result<TMap, TError> Map<TMap>(Func<TValue, TMap> map)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(map);

            return result.IsSuccess ? Result<TMap, TError>.Success(map(result.Value)) : Result<TMap, TError>.Failure(result.Error);
        }

        /// <summary>
        /// Binds a <see cref = "Result{TValue,TError}" /> instance to a <see cref = "Result{TValue,TError}" /> instance with a value of type <typeparamref name = "TMap" /> if the result is a success result.
        /// </summary>
        /// <returns>The bound <see cref = "Result{TValue,TError}" /> instance, or a <see cref = "Result{TValue,TError}" /> instance with the error of the result if the result is a failure result.</returns>
        public Result<TMap, TError> Bind<TMap>(Func<TValue, Result<TMap, TError>> bind)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(bind);

            return result.IsSuccess ? bind(result.Value) : Result<TMap, TError>.Failure(result.Error);
        }

        /// <summary>
        /// Binds a <see cref = "Result{TValue,TError}" /> instance to a <see cref = "Result{TError}" /> instance if the result is a success result.
        /// </summary>
        /// <returns>The bound <see cref = "Result{TError}" /> instance, or a <see cref = "Result{TError}" /> instance with the error of the result if the result is a failure result.</returns>
        public Result<TError> Bind(Func<TValue, Result<TError>> bind)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(bind);

            return result.IsSuccess ? bind(result.Value) : Result<TError>.Failure(result.Error);
        }

        /// <summary>
        /// Ensures that a <see cref = "Result{TValue,TError}" /> instance satisfies the predicate if the result is a success result.
        /// </summary>
        /// <returns>The result, or a <see cref = "Result{TValue,TError}" /> instance with the specified error if the predicate is not satisfied.</returns>
        public Result<TValue, TError> Ensure(Func<TValue, bool> predicate, TError error)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(error);

            return result.IsSuccess && !predicate(result.Value) ? Result<TValue, TError>.Failure(error) : result;
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

        /// <summary>
        /// Maps a <see cref = "Result{TError}" /> instance to a <see cref = "Result{TValue,TError}" /> instance with a value of type <typeparamref name = "TMap" /> if the result is a success result.
        /// </summary>
        /// <returns>A <see cref = "Result{TValue,TError}" /> instance with the mapped value, or with the error of the result if the result is a failure result.</returns>
        public Result<TMap, TError> Map<TMap>(Func<TMap> map)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(map);

            return result.IsSuccess ? Result<TMap, TError>.Success(map()) : Result<TMap, TError>.Failure(result.Error);
        }

        /// <summary>
        /// Binds a <see cref = "Result{TError}" /> instance to a <see cref = "Result{TValue,TError}" /> instance with a value of type <typeparamref name = "TMap" /> if the result is a success result.
        /// </summary>
        /// <returns>The bound <see cref = "Result{TValue,TError}" /> instance, or a <see cref = "Result{TValue,TError}" /> instance with the error of the result if the result is a failure result.</returns>
        public Result<TMap, TError> Bind<TMap>(Func<Result<TMap, TError>> bind)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(bind);

            return result.IsSuccess ? bind() : Result<TMap, TError>.Failure(result.Error);
        }

        /// <summary>
        /// Binds a <see cref = "Result{TError}" /> instance to a <see cref = "Result{TError}" /> instance if the result is a success result.
        /// </summary>
        /// <returns>The bound <see cref = "Result{TError}" /> instance, or a <see cref = "Result{TError}" /> instance with the error of the result if the result is a failure result.</returns>
        public Result<TError> Bind(Func<Result<TError>> bind)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(bind);

            return result.IsSuccess ? bind() : Result<TError>.Failure(result.Error);
        }

        /// <summary>
        /// Ensures that a <see cref = "Result{TError}" /> instance satisfies the predicate if the result is a success result.
        /// </summary>
        /// <returns>The result, or a <see cref = "Result{TError}" /> instance with the specified error if the predicate is not satisfied.</returns>
        public Result<TError> Ensure(Func<bool> predicate, TError error)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(error);

            return result.IsSuccess && !predicate() ? Result<TError>.Failure(error) : result;
        }
    }
}
