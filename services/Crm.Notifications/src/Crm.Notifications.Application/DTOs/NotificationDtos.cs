namespace Crm.Notifications.Application.DTOs;

public record CreateNotificationDto(
    Guid UserId,
    int Type,
    string Title,
    string Message,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null);

public record NotificationResponseDto(
    Guid Id,
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string Status,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    DateTime CreatedAt);

public record UnreadCountDto(int Count);
