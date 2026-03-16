using Dotnet.Foundation.Models;
using MongoDB.Driver;

namespace Dotnet.Foundation.Extensions;

public static class MongoDriverExtensions
{
    /// <summary>
    /// Returns asynchronously a page of items of type <typeparamref name = "TItem" /> from a Mongo Driver query.
    /// </summary>
    /// <returns>A page of items of type <typeparamref name = "TItem" />.</returns>
    public static async Task<Page<TItem>> ToPageAsync<TItem>(this IFindFluent<TItem, TItem> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 0);

        var totalItems = await query.CountDocumentsAsync(cancellationToken);

        var items = await query
                          .Skip((pageNumber - 1) * pageSize)
                          .Limit(pageSize)
                          .ToListAsync(cancellationToken);

        return new Page<TItem>(items, pageNumber, pageSize, totalItems);
    }
}
