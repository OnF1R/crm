using Crm.Audit.Application.Services;
using Crm.Audit.Domain.Interfaces;
using Crm.Audit.Infrastructure.Persistence;
using Crm.Audit.Infrastructure.Repositories;
using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Audit.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Audit")));
        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<AuditDbContext>());
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRepository<AuditLog>, Repository<AuditLog>>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AuditDbContext>());
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}
