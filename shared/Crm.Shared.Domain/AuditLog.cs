namespace Crm.Shared.Domain;

public class AuditLog : AggregateRoot
{
    public string ServiceName { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = null!;
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? Changes { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    private AuditLog() { }

    public static AuditLog Create(string serviceName, string entityName, Guid entityId, string action, Guid userId, string? userName = null, string? changes = null, string? ipAddress = null)
    {
        return new AuditLog
        {
            ServiceName = serviceName,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            UserName = userName,
            Changes = changes,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
    }
}
