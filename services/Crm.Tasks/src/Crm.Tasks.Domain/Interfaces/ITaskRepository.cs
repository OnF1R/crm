using Crm.Tasks.Domain.Entities;
using Crm.Tasks.Domain.Enums;
using Crm.Shared.Domain;

namespace Crm.Tasks.Domain.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IReadOnlyList<TaskItem>> GetByAssignedUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetByRelatedEntityAsync(RelatedEntityType entityType, Guid entityId, CancellationToken ct = default);
    Task<TaskItem?> GetWithCommentsAsync(Guid id, CancellationToken ct = default);
    Task AddCommentAsync(TaskComment comment, CancellationToken ct = default);
}
