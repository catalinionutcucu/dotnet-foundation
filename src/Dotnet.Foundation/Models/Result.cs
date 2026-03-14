namespace Dotnet.Foundation.Models;

public sealed class Result<TValue, TError>
{
    public ResultState State { get; }

    public TValue Value => State == ResultState.Success ? field : throw new InvalidOperationException($"Property '{nameof(Value)}' cannot be accessed on failure result.");

    public TError Error => State == ResultState.Failure ? field : throw new InvalidOperationException($"Property '{nameof(Error)}' cannot be accessed on success result.");

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

    public static Result<TValue, TError> Success(TValue value)
    {
        return new(value);
    }

    public static Result<TValue, TError> Failure(TError error)
    {
        return new(error);
    }

    public static implicit operator Result<TValue, TError>(TValue value)
    {
        return Success(value);
    }

    public static implicit operator Result<TValue, TError>(TError error)
    {
        return Failure(error);
    }
}

public sealed class Result<TError>
{
    public ResultState State { get; }

    public TError Error => State == ResultState.Failure ? field : throw new InvalidOperationException($"Property '{nameof(Error)}' cannot be accessed on success result.");

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

    public static Result<TError> Success()
    {
        return new();
    }

    public static Result<TError> Failure(TError error)
    {
        return new(error);
    }

    public static implicit operator Result<TError>(TError error)
    {
        return Failure(error);
    }
}

public enum ResultState
{
    Success,
    Failure
}
