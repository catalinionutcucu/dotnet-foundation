using System.Collections.Immutable;

namespace Dotnet.Foundation.Models;

public sealed class Page<TItem>
{
    public ImmutableArray<TItem> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public long TotalItems { get; }

    public int TotalPages { get; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public Page(IEnumerable<TItem> items, int pageNumber, int pageSize, long totalItems)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(totalItems, 0);

        Items = [ ..items ];
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
