using Crm.Clients.Domain.Entities;
using Crm.Shared.Domain;

namespace Crm.Clients.Domain.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<IReadOnlyList<Client>> GetByAssignedUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Client?> GetWithContactsAsync(Guid id, CancellationToken ct = default);
}
