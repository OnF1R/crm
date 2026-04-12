using Crm.Notifications.Domain.Entities;
using Crm.Shared.Domain;

namespace Crm.Notifications.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken ct = default);
}
