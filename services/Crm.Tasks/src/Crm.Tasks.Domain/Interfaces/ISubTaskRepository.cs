using Crm.Tasks.Domain.Entities;
using Crm.Shared.Domain;

namespace Crm.Tasks.Domain.Interfaces;

public interface ISubTaskRepository : IRepository<SubTask>
{
    Task<IReadOnlyList<SubTask>> GetByParentTaskIdAsync(Guid parentTaskId, CancellationToken ct = default);
}
