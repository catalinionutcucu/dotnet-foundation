using System.Collections.Immutable;

namespace Dotnet.Foundation.Models;

public sealed class RequestError
{
    public RequestErrorType Type { get; }

    public string Code { get; }

    public ImmutableArray<string> Issues { get; }

    private RequestError(RequestErrorType type, string code, params IEnumerable<string> issues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(issues);

        Type = type;
        Code = code;
        Issues = [ ..issues ];
    }

    /// <summary>
    /// Creates a <see cref = "RequestError" /> instance representing a request invalid error.
    /// </summary>
    /// <returns>A <see cref = "RequestError" /> instance representing a request invalid error.</returns>
    public static RequestError RequestInvalid(string code, params IEnumerable<string> issues)
    {
        return new(RequestErrorType.RequestInvalid, code, issues);
    }

    /// <summary>
    /// Creates a <see cref = "RequestError" /> instance representing a request not allowed error.
    /// </summary>
    /// <returns>A <see cref = "RequestError" /> instance representing a request not allowed error.</returns>
    public static RequestError RequestNotAllowed(string code, params IEnumerable<string> issues)
    {
        return new(RequestErrorType.RequestNotAllowed, code, issues);
    }

    /// <summary>
    /// Creates a <see cref = "RequestError" /> instance representing a resource not found error.
    /// </summary>
    /// <returns>A <see cref = "RequestError" /> instance representing a resource not found error.</returns>
    public static RequestError ResourceNotFound(string code, params IEnumerable<string> issues)
    {
        return new(RequestErrorType.ResourceNotFound, code, issues);
    }

    /// <summary>
    /// Creates a <see cref = "RequestError" /> instance representing a resource conflict error.
    /// </summary>
    /// <returns>A <see cref = "RequestError" /> instance representing a resource conflict error.</returns>
    public static RequestError ResourceConflict(string code, params IEnumerable<string> issues)
    {
        return new(RequestErrorType.ResourceConflict, code, issues);
    }
}

public enum RequestErrorType
{
    RequestInvalid,
    RequestNotAllowed,
    ResourceNotFound,
    ResourceConflict
}
