using Crm.Clients.Domain.Entities;
using Crm.Clients.Domain.Interfaces;
using Crm.Clients.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Clients.Infrastructure.Repositories;

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(ClientsDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<Client>> GetByAssignedUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbContext.Set<Client>()
            .Include(c => c.Contacts)
            .Where(c => c.AssignedUserId == userId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Client?> GetWithContactsAsync(Guid id, CancellationToken ct = default) =>
        await DbContext.Set<Client>()
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
}
