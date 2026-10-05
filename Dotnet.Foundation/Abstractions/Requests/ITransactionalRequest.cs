namespace Dotnet.Foundation.Abstractions.Requests;

/// <summary>
/// Marks a request handled in a database transaction, committed when the request succeeds and rolled back when it fails.
/// </summary>
public interface ITransactionalRequest;
