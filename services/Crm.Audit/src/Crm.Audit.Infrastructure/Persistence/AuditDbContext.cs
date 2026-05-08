using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Audit.Infrastructure.Persistence;

public class AuditDbContext : BaseDbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(200);
            entity.Property(e => e.Changes).HasMaxLength(5000);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.HasIndex(e => e.ServiceName);
            entity.HasIndex(e => e.EntityName);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
