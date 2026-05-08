namespace Crm.Audit.Domain.Interfaces;

public interface IAuditService
{
    Task LogAsync(string serviceName, string entityName, Guid entityId, string action, Guid userId, string? userName = null, string? changes = null, string? ipAddress = null, CancellationToken ct = default);
}
