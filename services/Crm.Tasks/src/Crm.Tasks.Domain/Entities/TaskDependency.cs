using Crm.Shared.Domain;

namespace Crm.Tasks.Domain.Entities;

public enum DependencyType
{
    FinishToStart = 1,  // Задача B начинается после завершения задачи A
    StartToStart = 2,    // Задача B начинается после начала задачи A
    FinishToFinish = 3   // Задача B завершается после завершения задачи A
}

public class TaskDependency : AggregateRoot
{
    public Guid PredecessorTaskId { get; set; }
    public Guid SuccessorTaskId { get; set; }
    public DependencyType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    private TaskDependency() { }

    public static TaskDependency Create(Guid predecessorTaskId, Guid successorTaskId, DependencyType type)
    {
        return new TaskDependency
        {
            PredecessorTaskId = predecessorTaskId,
            SuccessorTaskId = successorTaskId,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
    }
}
