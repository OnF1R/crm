using Crm.Clients.Domain.Entities;
using Crm.Clients.Domain.Interfaces;
using Crm.Clients.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Clients.Infrastructure.Repositories;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(ClientsDbContext dbContext) : base(dbContext) { }

    public new async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default) =>
        await DbContext.Set<Tag>()
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await DbContext.Set<Tag>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == name, ct);
}
