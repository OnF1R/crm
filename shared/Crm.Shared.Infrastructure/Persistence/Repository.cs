using Crm.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Shared.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : AggregateRoot
{
    protected readonly BaseDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public Repository(BaseDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking().ToListAsync(ct);

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        ISpecification<T> specification, CancellationToken ct = default) =>
        await specification.Apply(DbSet.AsNoTracking()).ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default) =>
        await DbSet.AddAsync(entity, ct);

    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
