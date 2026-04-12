using Crm.Notifications.Domain.Enums;
using Crm.Shared.Domain;

namespace Crm.Notifications.Domain.Entities;

public class Notification : AggregateRoot
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationStatus Status { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }

    private Notification() { }

    public static Notification Create(Guid userId, NotificationType type, string title, string message, string? relatedEntityType = null, Guid? relatedEntityId = null)
    {
        return new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Status = NotificationStatus.Непрочитано,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        Status = NotificationStatus.Прочитано;
    }
}
