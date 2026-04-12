using Crm.Notifications.Domain.Entities;
using Crm.Notifications.Domain.Enums;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Notifications.Infrastructure.Persistence;

public class NotificationsDbContext : BaseDbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Message).IsRequired();
            entity.HasIndex(e => e.UserId);
        });
    }
}
