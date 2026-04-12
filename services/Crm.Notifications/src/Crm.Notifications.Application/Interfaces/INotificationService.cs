using Crm.Notifications.Application.DTOs;
using Crm.Shared.DTOs;

namespace Crm.Notifications.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationResponseDto> CreateAsync(CreateNotificationDto dto, CancellationToken ct = default);
    Task<NotificationResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResponse<NotificationResponseDto>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<NotificationResponseDto> MarkAsReadAsync(Guid id, CancellationToken ct = default);
    Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
