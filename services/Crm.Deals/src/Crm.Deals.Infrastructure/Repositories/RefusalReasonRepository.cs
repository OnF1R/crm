using Crm.Deals.Domain.Entities;
using Crm.Deals.Domain.Interfaces;
using Crm.Deals.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Deals.Infrastructure.Repositories;

public class RefusalReasonRepository : Repository<RefusalReason>, IRefusalReasonRepository
{
    public RefusalReasonRepository(DealsDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<RefusalReason>> GetActiveAsync(CancellationToken ct = default) =>
        await DbContext.Set<RefusalReason>()
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    public async Task<RefusalReason?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await DbContext.Set<RefusalReason>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == name, ct);
}
