using Dotnet.Foundation.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Dotnet.Foundation.Implementations;

/// <summary>
/// Represents the current user implementing <see cref = "ICurrentUser" />, identified by the name identifier or subject claim of the user of the current HTTP request.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    public string? Id => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
}
