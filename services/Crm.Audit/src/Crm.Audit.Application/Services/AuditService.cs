using Crm.Audit.Application.DTOs;
using Crm.Audit.Domain.Interfaces;
using Crm.Shared.Domain;

namespace Crm.Audit.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IAuditLogRepository auditLogRepository, IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(string serviceName, string entityName, Guid entityId, string action, Guid userId, string? userName = null, string? changes = null, string? ipAddress = null, CancellationToken ct = default)
    {
        var auditLog = AuditLog.Create(serviceName, entityName, entityId, action, userId, userName, changes, ipAddress);
        await _auditLogRepository.AddAsync(auditLog, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
