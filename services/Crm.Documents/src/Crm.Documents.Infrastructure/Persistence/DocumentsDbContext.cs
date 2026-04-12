using Crm.Documents.Domain.Entities;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Documents.Infrastructure.Persistence;

public class DocumentsDbContext : BaseDbContext
{
    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options) : base(options) { }
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(200);
            entity.Property<string>("StoredFileName").HasMaxLength(500);
            entity.HasIndex(e => e.UploadedByUserId);
        });
    }
}
