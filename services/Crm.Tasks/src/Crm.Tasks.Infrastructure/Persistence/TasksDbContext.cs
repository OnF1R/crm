using Crm.Tasks.Domain.Entities;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Tasks.Infrastructure.Persistence;

public class TasksDbContext : BaseDbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options) { }
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskComment> Comments => Set<TaskComment>();
    public DbSet<SubTask> SubTasks => Set<SubTask>();
    public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.HasMany(e => e.Comments)
                .WithOne()
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.SubTasks)
                .WithOne()
                .HasForeignKey(st => st.ParentTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Dependencies)
                .WithOne()
                .HasForeignKey(d => d.SuccessorTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.ToTable("task_comments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(5000);
        });
        modelBuilder.Entity<SubTask>(entity =>
        {
            entity.ToTable("subtasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Order).HasDefaultValue(0);
            entity.HasIndex(e => e.ParentTaskId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        modelBuilder.Entity<TaskDependency>(entity =>
        {
            entity.ToTable("task_dependencies");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PredecessorTaskId);
            entity.HasIndex(e => e.SuccessorTaskId);
            entity.HasOne<TaskItem>()
                .WithMany()
                .HasForeignKey(e => e.PredecessorTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
