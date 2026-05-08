using Crm.Tasks.Application.Interfaces;
using Crm.Tasks.Application.Services;
using Crm.Tasks.Domain.Entities;
using Crm.Tasks.Domain.Interfaces;
using Crm.Tasks.Infrastructure.Persistence;
using Crm.Tasks.Infrastructure.Repositories;
using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Tasks.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTasksInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TasksDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Tasks")));
        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<TasksDbContext>());
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ISubTaskRepository, SubTaskRepository>();
        services.AddScoped<ITaskDependencyRepository, TaskDependencyRepository>();
        services.AddScoped<IRepository<TaskItem>, Repository<TaskItem>>();
        services.AddScoped<IRepository<SubTask>, Repository<SubTask>>();
        services.AddScoped<IRepository<TaskDependency>, Repository<TaskDependency>>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TasksDbContext>());
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
