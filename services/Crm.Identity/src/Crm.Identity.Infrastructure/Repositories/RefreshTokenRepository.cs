using Crm.Identity.Domain.Entities;
using Crm.Identity.Domain.Interfaces;
using Crm.Identity.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Infrastructure.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(IdentityDbContext dbContext) : base(dbContext) { }

    public async Task<RefreshToken?> GetByTokenAsync(string refreshToken, CancellationToken ct = default) =>
        await DbContext.Set<RefreshToken>()
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbContext.Set<RefreshToken>()
            .AsNoTracking()
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensAsync(Guid userId, CancellationToken ct = default) =>
        await DbContext.Set<RefreshToken>()
            .AsNoTracking()
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(ct);
}
