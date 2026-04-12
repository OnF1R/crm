using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Notifications.Infrastructure.Persistence;

public class NotificationsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=crm_notifications;Username=crm_admin;Password=crm_secret_2026");
        return new NotificationsDbContext(optionsBuilder.Options);
    }
}
