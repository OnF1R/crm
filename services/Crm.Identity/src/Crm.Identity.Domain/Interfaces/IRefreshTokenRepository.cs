using Crm.Identity.Domain.Entities;
using Crm.Shared.Domain;

namespace Crm.Identity.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensAsync(Guid userId, CancellationToken ct = default);
}
