using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Clients.Infrastructure.Persistence;

public class ClientsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ClientsDbContext>
{
    public ClientsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClientsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=crm_clients;Username=crm_admin;Password=crm_secret_2026");
        return new ClientsDbContext(optionsBuilder.Options);
    }
}
