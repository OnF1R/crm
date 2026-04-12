using Crm.Tasks.Domain.Entities;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Tasks.Infrastructure.Persistence;

public class TasksDbContext : BaseDbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options) { }
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
