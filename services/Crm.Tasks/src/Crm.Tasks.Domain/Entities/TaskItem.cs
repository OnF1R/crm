using Crm.Tasks.Domain.Enums;
using Crm.Shared.Domain;
using TaskStatus = Crm.Tasks.Domain.Enums.TaskStatus;

namespace Crm.Tasks.Domain.Entities;

public class TaskItem : AggregateRoot, IAuditable, ISoftDeletable
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public Guid AssignedUserId { get; set; }
    public RelatedEntityType? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    private readonly List<TaskComment> _comments = [];
    public IReadOnlyList<TaskComment> Comments => _comments.AsReadOnly();

    private readonly List<SubTask> _subTasks = [];
    public IReadOnlyList<SubTask> SubTasks => _subTasks.AsReadOnly();

    private readonly List<TaskDependency> _dependencies = [];
    public IReadOnlyList<TaskDependency> Dependencies => _dependencies.AsReadOnly();

    private TaskItem() { }

    public static TaskItem Create(string title, string? description, DateTime? dueDate, TaskPriority priority, Guid assignedUserId, Guid createdBy, RelatedEntityType? relatedEntityType = null, Guid? relatedEntityId = null)
    {
        return new TaskItem
        {
            Title = title,
            Description = description,
            DueDate = dueDate,
            Priority = priority,
            Status = TaskStatus.Новая,
            AssignedUserId = assignedUserId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        };
    }

    public void AddComment(TaskComment comment) => _comments.Add(comment);

    public SubTask AddSubTask(string title, string? description, DateTime? dueDate, TaskPriority priority, Guid assignedUserId, Guid createdBy, int order = 0)
    {
        var subTask = SubTask.Create(Id, title, description, dueDate, priority, assignedUserId, createdBy, order);
        _subTasks.Add(subTask);
        return subTask;
    }

    public void RemoveSubTask(Guid subTaskId)
    {
        var subTask = _subTasks.FirstOrDefault(st => st.Id == subTaskId);
        if (subTask != null)
        {
            _subTasks.Remove(subTask);
        }
    }

    public void UpdateSubTaskOrder()
    {
        for (int i = 0; i < _subTasks.Count; i++)
        {
            _subTasks[i].Order = i;
        }
    }

    public TaskDependency AddDependency(Guid predecessorTaskId, DependencyType type)
    {
        var dependency = TaskDependency.Create(predecessorTaskId, Id, type);
        _dependencies.Add(dependency);
        return dependency;
    }

    public void RemoveDependency(Guid dependencyId)
    {
        var dependency = _dependencies.FirstOrDefault(d => d.Id == dependencyId);
        if (dependency != null)
        {
            _dependencies.Remove(dependency);
        }
    }

    public void Update(string title, string? description, DateTime? dueDate, TaskPriority priority, Guid assignedUserId)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        Priority = priority;
        AssignedUserId = assignedUserId;
    }

    public void SetStatus(TaskStatus newStatus)
    {
        Status = newStatus;
    }
}
