using Crm.Notifications.Application.DTOs;
using Crm.Notifications.Application.Interfaces;
using Crm.Notifications.Domain.Enums;
using Crm.Notifications.Domain.Interfaces;
using Crm.Shared.Domain;
using Crm.Shared.DTOs;

namespace Crm.Notifications.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<NotificationResponseDto> CreateAsync(CreateNotificationDto dto, CancellationToken ct = default)
    {
        var notification = Domain.Entities.Notification.Create(dto.UserId, 
            (NotificationType)dto.Type, dto.Title, dto.Message, dto.RelatedEntityType, dto.RelatedEntityId);
        await _notificationRepository.AddAsync(notification, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(notification);
    }

    public async Task<NotificationResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Уведомление не найдено");
        return MapToResponse(notification);
    }

    public async Task<PagedResponse<NotificationResponseDto>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, ct);
        var total = notifications.Count;
        var paged = notifications.OrderByDescending(n => n.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList();
        return new PagedResponse<NotificationResponseDto>(paged, total, page, pageSize);
    }

    public async Task<NotificationResponseDto> MarkAsReadAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Уведомление не найдено");
        notification.MarkAsRead();
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(notification);
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var count = await _notificationRepository.GetUnreadCountByUserIdAsync(userId, ct);
        return new UnreadCountDto(count);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Уведомление не найдено");
        await _notificationRepository.DeleteAsync(notification, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static NotificationResponseDto MapToResponse(Domain.Entities.Notification n) => new(
        n.Id, n.UserId, n.Type.ToString(), n.Title, n.Message, n.Status.ToString(),
        n.RelatedEntityType, n.RelatedEntityId, n.CreatedAt);
}
