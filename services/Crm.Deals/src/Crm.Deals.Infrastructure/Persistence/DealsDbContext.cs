using Crm.Deals.Domain.Entities;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Deals.Infrastructure.Persistence;

public class DealsDbContext : BaseDbContext
{
    public DealsDbContext(DbContextOptions<DealsDbContext> options) : base(options) { }
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<DealStageHistory> DealStageHistories => Set<DealStageHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Deal>(entity =>
        {
            entity.ToTable("deals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasMany(e => e.StageHistory)
                .WithOne()
                .HasForeignKey(h => h.DealId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DealStageHistory>(entity =>
        {
            entity.ToTable("deal_stage_history");
            entity.HasKey(e => e.Id);
        });
    }
}
