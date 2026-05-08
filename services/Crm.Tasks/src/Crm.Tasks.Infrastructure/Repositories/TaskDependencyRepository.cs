using Crm.Tasks.Domain.Entities;
using Crm.Tasks.Domain.Interfaces;
using Crm.Tasks.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Tasks.Infrastructure.Repositories;

public class TaskDependencyRepository : Repository<TaskDependency>, ITaskDependencyRepository
{
    public TaskDependencyRepository(TasksDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<TaskDependency>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default) =>
        await DbContext.Set<TaskDependency>()
            .AsNoTracking()
            .Where(d => d.PredecessorTaskId == taskId || d.SuccessorTaskId == taskId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TaskDependency>> GetPredecessorsAsync(Guid taskId, CancellationToken ct = default) =>
        await DbContext.Set<TaskDependency>()
            .AsNoTracking()
            .Where(d => d.SuccessorTaskId == taskId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TaskDependency>> GetSuccessorsAsync(Guid taskId, CancellationToken ct = default) =>
        await DbContext.Set<TaskDependency>()
            .AsNoTracking()
            .Where(d => d.PredecessorTaskId == taskId)
            .ToListAsync(ct);
}
