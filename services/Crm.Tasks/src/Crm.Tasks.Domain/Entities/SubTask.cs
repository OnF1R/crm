using Crm.Tasks.Domain.Enums;
using Crm.Shared.Domain;
using TaskStatus = Crm.Tasks.Domain.Enums.TaskStatus;

namespace Crm.Tasks.Domain.Entities;

public class SubTask : AggregateRoot, IAuditable, ISoftDeletable
{
    public Guid ParentTaskId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public Guid AssignedUserId { get; set; }
    public int Order { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    private SubTask() { }

    public static SubTask Create(Guid parentTaskId, string title, string? description, DateTime? dueDate, TaskPriority priority, Guid assignedUserId, Guid createdBy, int order = 0)
    {
        return new SubTask
        {
            ParentTaskId = parentTaskId,
            Title = title,
            Description = description,
            DueDate = dueDate,
            Priority = priority,
            Status = TaskStatus.Новая,
            AssignedUserId = assignedUserId,
            Order = order,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string? description, DateTime? dueDate, TaskPriority priority, Guid assignedUserId, int order)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        Priority = priority;
        AssignedUserId = assignedUserId;
        Order = order;
    }

    public void SetStatus(TaskStatus newStatus)
    {
        Status = newStatus;
    }
}
