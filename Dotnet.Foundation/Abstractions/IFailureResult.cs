namespace Dotnet.Foundation.Abstractions;

/// <summary>
/// Defines a result of type <typeparamref name = "TResult" /> that can be a failure result with an error of type <typeparamref name = "TError" />.
/// </summary>
public interface IFailureResult<TResult, TError>
{
    public bool IsFailure { get; }

    public TError Error { get; }

    /// <summary>
    /// Creates a <typeparamref name = "TResult" /> instance representing a failure result.
    /// </summary>
    /// <returns>A <typeparamref name = "TResult" /> instance representing a failure result.</returns>
    public static abstract TResult Failure(TError error);
}
