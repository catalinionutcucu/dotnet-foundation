using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Dotnet.Foundation.Models;

/// <summary>
/// Represents an error that occurred while processing a request.
/// </summary>
public sealed class RequestError
{
    public RequestErrorType Type { get; }

    public string Code { get; }

    public ImmutableArray<string> Issues { get; }

    [JsonConstructor]
    private RequestError(RequestErrorType type, string code, ImmutableArray<string> issues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Type = type;
        Code = code;
        Issues = issues.IsDefault ? [ ] : issues;
    }

    /// <summary>
    /// Creates a <see cref = "RequestError" /> instance representing a request invalid error.
    /// </summary>
    /// <returns>A <see cref = "RequestError" /> instance representing a request invalid error.</returns>
    public static RequestError RequestInvalid(string code, params IEnumerable<string> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return new(RequestErrorType.RequestInvalid, code, [ ..issues ]);
    }

    /// <summary>
    /// Creates a <see cref = "RequestError" /> instance representing a request not allowed error.
    /// </summary>
    /// <returns>A <see cref = "RequestError" /> instance representing a request not allowed error.</returns>
    public static RequestError RequestNotAllowed(string code, params IEnumerable<string> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return new(RequestErrorType.RequestNotAllowed, code, [ ..issues ]);
    }

    /// <summary>
    /// Creates a <see cref = "RequestError" /> instance representing a resource not found error.
    /// </summary>
    /// <returns>A <see cref = "RequestError" /> instance representing a resource not found error.</returns>
    public static RequestError ResourceNotFound(string code, params IEnumerable<string> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return new(RequestErrorType.ResourceNotFound, code, [ ..issues ]);
    }

    /// <summary>
    /// Creates a <see cref = "RequestError" /> instance representing a resource conflict error.
    /// </summary>
    /// <returns>A <see cref = "RequestError" /> instance representing a resource conflict error.</returns>
    public static RequestError ResourceConflict(string code, params IEnumerable<string> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return new(RequestErrorType.ResourceConflict, code, [ ..issues ]);
    }
}

/// <summary>
/// Specifies the type of a <see cref = "RequestError" /> instance.
/// </summary>
public enum RequestErrorType
{
    RequestInvalid,
    RequestNotAllowed,
    ResourceNotFound,
    ResourceConflict
}
