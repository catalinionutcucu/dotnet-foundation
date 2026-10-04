using Microsoft.AspNetCore.Routing;

namespace Dotnet.Foundation.Abstractions;

/// <summary>
/// Defines an endpoint that maps itself to an endpoint route builder.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the endpoint route builder.
    /// </summary>
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder);
}
