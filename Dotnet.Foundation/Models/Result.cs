namespace Dotnet.Foundation.Models;

public sealed class Result<TValue, TError>
{
    public ResultState State { get; }

    public TValue Value => State == ResultState.Success ?
        field :
        throw new InvalidOperationException($"Property '{nameof(Value)}' cannot be accessed on failure result.");

    public TError Error => State == ResultState.Failure ?
        field :
        throw new InvalidOperationException($"Property '{nameof(Error)}' cannot be accessed on success result.");

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

    public static implicit operator Result<TValue, TError>(TValue value)
    {
        return Success(value);
    }
}

public sealed class Result<TError>
{
    public ResultState State { get; }

    public TError Error => State == ResultState.Failure ?
        field :
        throw new InvalidOperationException($"Property '{nameof(Error)}' cannot be accessed on success result.");

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
}

public enum ResultState
{
    Success,
    Failure
}
