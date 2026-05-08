using Crm.Audit.Domain.Interfaces;
using Crm.Audit.Infrastructure.Persistence;
using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Audit.Infrastructure.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AuditDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<AuditLog>> GetByServiceAsync(string serviceName, CancellationToken ct = default) =>
        await DbContext.Set<AuditLog>()
            .AsNoTracking()
            .Where(log => log.ServiceName == serviceName)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        await DbContext.Set<AuditLog>()
            .AsNoTracking()
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken ct = default) =>
        await DbContext.Set<AuditLog>()
            .AsNoTracking()
            .Where(log => log.EntityName == entityName && log.EntityId == entityId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default) =>
        await DbContext.Set<AuditLog>()
            .AsNoTracking()
            .Where(log => log.CreatedAt >= from && log.CreatedAt <= to)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(ct);
}
