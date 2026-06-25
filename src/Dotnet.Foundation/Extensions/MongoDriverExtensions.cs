using Dotnet.Foundation.Models;
using MongoDB.Driver;

namespace Dotnet.Foundation.Extensions;

public static class MongoDriverExtensions
{
    /// <summary>
    /// Returns a page of items of type <typeparamref name = "TItem" /> from a Mongo Driver query.
    /// </summary>
    /// <returns>A page of items of type <typeparamref name = "TItem" />.</returns>
    public static Page<TItem> ToPage<TItem>(this IFindFluent<TItem, TItem> query, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        var totalItems = query.CountDocuments();

        var items = query.Skip((pageNumber - 1) * pageSize)
                         .Limit(pageSize)
                         .ToList();

        return new Page<TItem>(items, pageNumber, pageSize, totalItems);
    }

    /// <summary>
    /// Returns asynchronously a page of items of type <typeparamref name = "TItem" /> from a Mongo Driver query.
    /// </summary>
    /// <returns>A task that produces a page of items of type <typeparamref name = "TItem" />.</returns>
    public static async Task<Page<TItem>> ToPageAsync<TItem>(this IFindFluent<TItem, TItem> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        var totalItems = await query.CountDocumentsAsync(cancellationToken)
                                    .ConfigureAwait(false);

        var items = await query.Skip((pageNumber - 1) * pageSize)
                               .Limit(pageSize)
                               .ToListAsync(cancellationToken)
                               .ConfigureAwait(false);

        return new Page<TItem>(items, pageNumber, pageSize, totalItems);
    }
}
