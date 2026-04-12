using Crm.Analytics.Domain.Entities;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Analytics.Infrastructure.Persistence;

public class AnalyticsDbContext : BaseDbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Parameters).IsRequired();
            entity.Property(e => e.Data).IsRequired();
        });
        modelBuilder.Entity<DashboardWidget>(entity =>
        {
            entity.ToTable("dashboard_widgets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WidgetType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.UserId);
        });
    }
}
