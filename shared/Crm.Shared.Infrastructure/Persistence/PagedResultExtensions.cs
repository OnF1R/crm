using Crm.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Shared.Infrastructure.Persistence;

public static class PagedResultExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken ct = default)
    {
        int totalCount = await query.CountAsync(ct);
        IReadOnlyList<T> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new PagedResult<T>(items, totalCount, page, pageSize);
    }
}
