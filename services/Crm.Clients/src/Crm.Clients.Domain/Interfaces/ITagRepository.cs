using Crm.Clients.Domain.Entities;
using Crm.Shared.Domain;

namespace Crm.Clients.Domain.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default);
}
