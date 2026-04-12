using Crm.Clients.Application.Interfaces;
using Crm.Clients.Application.Services;
using Crm.Clients.Domain.Entities;
using Crm.Clients.Domain.Interfaces;
using Crm.Clients.Infrastructure.Persistence;
using Crm.Clients.Infrastructure.Repositories;
using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Clients.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ClientsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Clients")));

        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<ClientsDbContext>());
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IRepository<Client>, Repository<Client>>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClientsDbContext>());
        services.AddScoped<IClientService, ClientService>();

        return services;
    }
}
