using Crm.Shared.Domain;

namespace Crm.Audit.Domain.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByServiceAsync(string serviceName, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
