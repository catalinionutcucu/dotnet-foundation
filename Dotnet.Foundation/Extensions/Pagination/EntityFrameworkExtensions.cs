using Dotnet.Foundation.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.Foundation.Extensions.Pagination;

/// <summary>
/// Provides extension members for paginating Entity Framework queries.
/// </summary>
public static class EntityFrameworkExtensions
{
    extension<TItem>(IQueryable<TItem> query)
    {
        /// <summary>
        /// Returns a page of items of type <typeparamref name = "TItem" /> from an Entity Framework query.
        /// </summary>
        /// <returns>A page of items of type <typeparamref name = "TItem" />.</returns>
        public Page<TItem> ToPage(int pageNumber, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

            var totalItems = query.Count();

            var items = query.Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            return new(items, pageNumber, pageSize, totalItems);
        }

        /// <summary>
        /// Asynchronously returns a page of items of type <typeparamref name = "TItem" /> from an Entity Framework query.
        /// </summary>
        /// <returns>A page of items of type <typeparamref name = "TItem" />.</returns>
        public async Task<Page<TItem>> ToPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

            var totalItems = await query.CountAsync(cancellationToken)
                                        .ConfigureAwait(false);

            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(cancellationToken)
                                   .ConfigureAwait(false);

            return new(items, pageNumber, pageSize, totalItems);
        }
    }
}
