using Crm.Identity.Domain.Entities;
using Crm.Identity.Domain.Interfaces;
using Crm.Identity.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(IdentityDbContext dbContext) : base(dbContext) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await DbContext.Set<User>().FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        await DbContext.Set<User>().AnyAsync(u => u.Email == email, ct);
}
