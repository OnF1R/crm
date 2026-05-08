using Crm.Tasks.Domain.Entities;
using Crm.Tasks.Domain.Interfaces;
using Crm.Tasks.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Tasks.Infrastructure.Repositories;

public class SubTaskRepository : Repository<SubTask>, ISubTaskRepository
{
    public SubTaskRepository(TasksDbContext dbContext) : base(dbContext) { }

    public override Task AddAsync(SubTask entity, CancellationToken ct = default)
    {
        DbContext.Entry(entity).State = EntityState.Added;
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<SubTask>> GetByParentTaskIdAsync(Guid parentTaskId, CancellationToken ct = default) =>
        await DbContext.Set<SubTask>()
            .AsNoTracking()
            .Where(st => st.ParentTaskId == parentTaskId)
            .OrderBy(st => st.Order)
            .ToListAsync(ct);
}
