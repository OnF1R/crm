using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crm.Documents.Infrastructure.Persistence;

public class DocumentsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DocumentsDbContext>
{
    public DocumentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DocumentsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=crm_documents;Username=crm_admin;Password=crm_secret_2026");
        return new DocumentsDbContext(optionsBuilder.Options);
    }
}
