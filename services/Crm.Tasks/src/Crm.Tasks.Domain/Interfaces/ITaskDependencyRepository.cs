using Crm.Tasks.Domain.Entities;
using Crm.Shared.Domain;

namespace Crm.Tasks.Domain.Interfaces;

public interface ITaskDependencyRepository : IRepository<TaskDependency>
{
    Task<IReadOnlyList<TaskDependency>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskDependency>> GetPredecessorsAsync(Guid taskId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskDependency>> GetSuccessorsAsync(Guid taskId, CancellationToken ct = default);
}
