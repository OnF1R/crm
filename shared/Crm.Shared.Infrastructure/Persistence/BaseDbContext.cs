using Crm.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Crm.Shared.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext, IUnitOfWork
{
    protected BaseDbContext(DbContextOptions options) : base(options) { }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = DateTime.UtcNow;
                        break;
                }
            }

            if (entry.Entity is ISoftDeletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Property(nameof(ISoftDeletable.IsDeleted)).CurrentValue = true;
                entry.Property(nameof(ISoftDeletable.DeletedAt)).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(ct);
    }
}
