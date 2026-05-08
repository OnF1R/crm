using Crm.Deals.Domain.Entities;
using Crm.Shared.Domain;

namespace Crm.Deals.Domain.Interfaces;

public interface IRefusalReasonRepository : IRepository<RefusalReason>
{
    Task<IReadOnlyList<RefusalReason>> GetActiveAsync(CancellationToken ct = default);
    Task<RefusalReason?> GetByNameAsync(string name, CancellationToken ct = default);
}
