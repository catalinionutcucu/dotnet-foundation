namespace Dotnet.Foundation.Models;

/// <summary>
/// Represents a request for a page of items, with the page number and the page size clamped to at least 1.
/// </summary>
public sealed class PageRequest
{
    public int PageNumber { get; }

    public int PageSize { get; }

    public PageRequest(int pageNumber = 1, int pageSize = 10)
    {
        PageNumber = Math.Max(pageNumber, 1);
        PageSize = Math.Max(pageSize, 1);
    }
}
