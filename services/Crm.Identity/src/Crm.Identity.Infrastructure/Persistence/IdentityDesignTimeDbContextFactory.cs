using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Identity.Infrastructure.Persistence;

public class IdentityDesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=crm_identity;Username=crm_admin;Password=crm_secret_2026");
        return new IdentityDbContext(optionsBuilder.Options);
    }
}
