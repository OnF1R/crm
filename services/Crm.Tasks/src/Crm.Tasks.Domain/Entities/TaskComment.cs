using Crm.Shared.Domain;

namespace Crm.Tasks.Domain.Entities;

public class TaskComment : Entity
{
    public Guid TaskId { get; set; }
    public string Content { get; set; } = null!;
    public Guid AuthorUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    private TaskComment() { }

    public static TaskComment Create(Guid taskId, string content, Guid authorUserId)
    {
        return new TaskComment
        {
            TaskId = taskId,
            Content = content,
            AuthorUserId = authorUserId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
