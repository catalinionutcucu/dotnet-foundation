using Dotnet.Foundation.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.Foundation.Extensions;

public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Returns asynchronously a page of items of type <typeparamref name = "TItem" /> from an Entity Framework query.
    /// </summary>
    /// <returns>A page of items of type <typeparamref name = "TItem" />.</returns>
    public static async Task<Page<TItem>> ToPageAsync<TItem>(this IQueryable<TItem> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 0);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
                          .Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync(cancellationToken);

        return new Page<TItem>(items, pageNumber, pageSize, totalItems);
    }
}
