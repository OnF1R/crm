using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Analytics.Infrastructure.Persistence;

public class AnalyticsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AnalyticsDbContext>
{
    public AnalyticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AnalyticsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=crm_analytics;Username=crm_admin;Password=crm_secret_2026");
        return new AnalyticsDbContext(optionsBuilder.Options);
    }
}
