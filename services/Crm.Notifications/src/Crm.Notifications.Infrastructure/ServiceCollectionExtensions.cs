using Crm.Notifications.Application.Interfaces;
using Crm.Notifications.Application.Services;
using Crm.Notifications.Domain.Entities;
using Crm.Notifications.Domain.Interfaces;
using Crm.Notifications.Infrastructure.Persistence;
using Crm.Notifications.Infrastructure.Repositories;
using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Notifications.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Notifications")));
        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<NotificationsDbContext>());
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IRepository<Notification>, Repository<Notification>>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NotificationsDbContext>());
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}
