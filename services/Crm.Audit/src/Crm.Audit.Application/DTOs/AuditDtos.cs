namespace Crm.Audit.Application.DTOs;

public record CreateAuditLogDto(
    string ServiceName,
    string EntityName,
    Guid EntityId,
    string Action,
    Guid UserId,
    string? UserName = null,
    string? Changes = null,
    string? IpAddress = null);

public record AuditLogResponseDto(
    Guid Id,
    string ServiceName,
    string EntityName,
    Guid EntityId,
    string Action,
    Guid UserId,
    string? UserName,
    string? Changes,
    string? IpAddress,
    DateTime CreatedAt);
