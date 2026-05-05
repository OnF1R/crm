using Crm.Deals.Domain.Entities;
using Crm.Deals.Domain.Interfaces;
using Crm.Deals.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Deals.Infrastructure.Repositories;

public class DealRepository : Repository<Deal>, IDealRepository
{
    public DealRepository(DealsDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<Deal>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default) =>
        await DbContext.Set<Deal>().AsNoTracking().Where(d => d.ClientId == clientId).ToListAsync(ct);

    public async Task<IReadOnlyList<Deal>> GetByAssignedUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbContext.Set<Deal>().AsNoTracking().Where(d => d.AssignedUserId == userId).ToListAsync(ct);

    public async Task<Deal?> GetWithHistoryAsync(Guid id, CancellationToken ct = default) =>
        await DbContext.Set<Deal>().Include(d => d.StageHistory).FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task AddStageHistoryAsync(DealStageHistory history, CancellationToken ct = default) =>
        await DbContext.Set<DealStageHistory>().AddAsync(history, ct);
}
