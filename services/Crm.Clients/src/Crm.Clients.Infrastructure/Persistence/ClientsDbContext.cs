using Crm.Clients.Domain.Entities;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Clients.Infrastructure.Persistence;

public class ClientsDbContext : BaseDbContext
{
    public ClientsDbContext(DbContextOptions<ClientsDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("clients");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Inn).HasMaxLength(12);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.Industry).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasMany(e => e.Contacts)
                .WithOne()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("contacts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Position).HasMaxLength(200);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Client>()
            .HasMany(c => c.Tags)
            .WithMany()
            .UsingEntity(j => j.ToTable("client_tags"));
    }
}
