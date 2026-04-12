using Crm.Tasks.Domain.Entities;
using Crm.Tasks.Domain.Enums;
using Crm.Tasks.Domain.Interfaces;
using Crm.Tasks.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Tasks.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(TasksDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<TaskItem>> GetByAssignedUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbContext.Set<TaskItem>().AsNoTracking().Where(t => t.AssignedUserId == userId).ToListAsync(ct);

    public async Task<IReadOnlyList<TaskItem>> GetByRelatedEntityAsync(RelatedEntityType entityType, Guid entityId, CancellationToken ct = default) =>
        await DbContext.Set<TaskItem>().AsNoTracking()
            .Where(t => t.RelatedEntityType == entityType && t.RelatedEntityId == entityId)
            .ToListAsync(ct);

    public async Task<TaskItem?> GetWithCommentsAsync(Guid id, CancellationToken ct = default) =>
        await DbContext.Set<TaskItem>()
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddCommentAsync(TaskComment comment, CancellationToken ct = default)
    {
        await DbContext.Set<TaskComment>().AddAsync(comment, ct);
    }
}
