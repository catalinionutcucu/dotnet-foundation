using Microsoft.AspNetCore.Routing;

namespace Dotnet.Foundation.Abstractions.Endpoints;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder);
}
