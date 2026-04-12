using Crm.Identity.Domain.Entities;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Infrastructure.Persistence;

public class IdentityDbContext : BaseDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(512);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
