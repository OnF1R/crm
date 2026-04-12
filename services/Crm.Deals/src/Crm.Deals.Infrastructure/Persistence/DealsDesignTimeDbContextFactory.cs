using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Deals.Infrastructure.Persistence;

public class DealsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DealsDbContext>
{
    public DealsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DealsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=crm_deals;Username=crm_admin;Password=crm_secret_2026");
        return new DealsDbContext(optionsBuilder.Options);
    }
}
