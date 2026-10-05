using Dotnet.Foundation.Abstractions;
using System.Text.Json.Serialization;

namespace Dotnet.Foundation.Models;

/// <summary>
/// Represents the result of an operation with a value of type <typeparamref name = "TValue" /> on success or an error of type <typeparamref name = "TError" /> on failure.
/// </summary>
[JsonConverter(typeof(ResultJsonConverterFactory))]
public sealed class Result<TValue, TError> : IFailureResult<Result<TValue, TError>, TError>
{
    public ResultState State { get; }

    public TValue Value => State is ResultState.Success ?
        field! :
        throw new InvalidOperationException($"Property '{nameof(Value)}' cannot be accessed on a failure result.");

    public TError Error => State is ResultState.Failure ?
        field! :
        throw new InvalidOperationException($"Property '{nameof(Error)}' cannot be accessed on a success result.");

    public bool IsSuccess => State is ResultState.Success;

    public bool IsFailure => State is ResultState.Failure;

    private Result(TValue value)
    {
        State = ResultState.Success;
        Value = value;
    }

    private Result(TError error)
    {
        State = ResultState.Failure;
        Error = error;
    }

    /// <summary>
    /// Creates a <see cref = "Result{TValue,TError}" /> instance representing a success result.
    /// </summary>
    /// <returns>A <see cref = "Result{TValue,TError}" /> instance representing a success result.</returns>
    public static Result<TValue, TError> Success(TValue value)
    {
        return new(value);
    }

    /// <summary>
    /// Creates a <see cref = "Result{TValue,TError}" /> instance representing a failure result.
    /// </summary>
    /// <returns>A <see cref = "Result{TValue,TError}" /> instance representing a failure result.</returns>
    public static Result<TValue, TError> Failure(TError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new(error);
    }

    /// <summary>
    /// Converts a value of type <typeparamref name = "TValue" /> to a <see cref = "Result{TValue,TError}" /> instance representing a success result.
    /// </summary>
    /// <returns>A <see cref = "Result{TValue,TError}" /> instance representing a success result.</returns>
    public static implicit operator Result<TValue, TError>(TValue value)
    {
        return Success(value);
    }

    /// <summary>
    /// Converts an error of type <typeparamref name = "TError" /> to a <see cref = "Result{TValue,TError}" /> instance representing a failure result.
    /// </summary>
    /// <returns>A <see cref = "Result{TValue,TError}" /> instance representing a failure result.</returns>
    public static implicit operator Result<TValue, TError>(TError error)
    {
        return Failure(error);
    }
}

/// <summary>
/// Represents the result of an operation without a value on success or with an error of type <typeparamref name = "TError" /> on failure.
/// </summary>
[JsonConverter(typeof(ResultJsonConverterFactory))]
public sealed class Result<TError> : IFailureResult<Result<TError>, TError>
{
    public ResultState State { get; }

    public TError Error => State is ResultState.Failure ?
        field! :
        throw new InvalidOperationException($"Property '{nameof(Error)}' cannot be accessed on a success result.");

    public bool IsSuccess => State is ResultState.Success;

    public bool IsFailure => State is ResultState.Failure;

    private Result()
    {
        State = ResultState.Success;
    }

    private Result(TError error)
    {
        State = ResultState.Failure;
        Error = error;
    }

    /// <summary>
    /// Creates a <see cref = "Result{TError}" /> instance representing a success result.
    /// </summary>
    /// <returns>A <see cref = "Result{TError}" /> instance representing a success result.</returns>
    public static Result<TError> Success()
    {
        return new();
    }

    /// <summary>
    /// Creates a <see cref = "Result{TError}" /> instance representing a failure result.
    /// </summary>
    /// <returns>A <see cref = "Result{TError}" /> instance representing a failure result.</returns>
    public static Result<TError> Failure(TError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new(error);
    }

    /// <summary>
    /// Converts an error of type <typeparamref name = "TError" /> to a <see cref = "Result{TError}" /> instance representing a failure result.
    /// </summary>
    /// <returns>A <see cref = "Result{TError}" /> instance representing a failure result.</returns>
    public static implicit operator Result<TError>(TError error)
    {
        return Failure(error);
    }
}

/// <summary>
/// Specifies the state of a <see cref = "Result{TValue,TError}" /> or <see cref = "Result{TError}" /> instance.
/// </summary>
public enum ResultState
{
    Success,
    Failure
}
