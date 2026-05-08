using Crm.Deals.Application.Interfaces;
using Crm.Deals.Application.Services;
using Crm.Deals.Domain.Entities;
using Crm.Deals.Domain.Interfaces;
using Crm.Deals.Infrastructure.Persistence;
using Crm.Deals.Infrastructure.Repositories;
using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Deals.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDealsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DealsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Deals")));
        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<DealsDbContext>());
        services.AddScoped<IDealRepository, DealRepository>();
        services.AddScoped<IRefusalReasonRepository, RefusalReasonRepository>();
        services.AddScoped<IRepository<Deal>, Repository<Deal>>();
        services.AddScoped<IRepository<RefusalReason>, Repository<RefusalReason>>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DealsDbContext>());
        services.AddScoped<IDealService, DealService>();
        return services;
    }
}
