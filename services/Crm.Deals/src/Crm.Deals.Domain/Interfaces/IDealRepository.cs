using Crm.Deals.Domain.Entities;
using Crm.Shared.Domain;

namespace Crm.Deals.Domain.Interfaces;

public interface IDealRepository : IRepository<Deal>
{
    Task<IReadOnlyList<Deal>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default);
    Task<IReadOnlyList<Deal>> GetByAssignedUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Deal?> GetWithHistoryAsync(Guid id, CancellationToken ct = default);
}
