using Crm.Analytics.Application.Interfaces;
using Crm.Analytics.Application.Services;
using Crm.Analytics.Application.Options;
using Crm.Analytics.Domain.Entities;
using Crm.Analytics.Domain.Interfaces;
using Crm.Analytics.Infrastructure.Persistence;
using Crm.Analytics.Infrastructure.Repositories;
using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Analytics.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalyticsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Analytics")));
        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<AnalyticsDbContext>());
        services.Configure<AnalyticsServiceOptions>(configuration.GetSection("AnalyticsService"));
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IRepository<Report>, Repository<Report>>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AnalyticsDbContext>());
        services.AddHttpClient("analytics-services");
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        return services;
    }
}
