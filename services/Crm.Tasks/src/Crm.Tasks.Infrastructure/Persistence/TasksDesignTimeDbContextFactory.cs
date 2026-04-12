using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Tasks.Infrastructure.Persistence;

public class TasksDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TasksDbContext>
{
    public TasksDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TasksDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=crm_tasks;Username=crm_admin;Password=crm_secret_2026");
        return new TasksDbContext(optionsBuilder.Options);
    }
}
