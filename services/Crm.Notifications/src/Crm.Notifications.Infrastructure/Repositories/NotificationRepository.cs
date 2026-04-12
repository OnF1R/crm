using Crm.Notifications.Domain.Entities;
using Crm.Notifications.Domain.Enums;
using Crm.Notifications.Domain.Interfaces;
using Crm.Notifications.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Notifications.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(NotificationsDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbContext.Set<Notification>().AsNoTracking().Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync(ct);

    public async Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbContext.Set<Notification>().AsNoTracking().Where(n => n.UserId == userId && n.Status == NotificationStatus.Непрочитано).ToListAsync(ct);

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbContext.Set<Notification>().AsNoTracking().CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Непрочитано, ct);
}
