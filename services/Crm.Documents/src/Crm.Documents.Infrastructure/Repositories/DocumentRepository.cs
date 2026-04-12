using Crm.Documents.Domain.Entities;
using Crm.Documents.Domain.Interfaces;
using Crm.Documents.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Documents.Infrastructure.Repositories;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(DocumentsDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<Document>> GetByRelatedEntityAsync(string entityType, Guid entityId, CancellationToken ct = default) =>
        await DbContext.Set<Document>().AsNoTracking()
            .Where(d => d.RelatedEntityType == entityType && d.RelatedEntityId == entityId)
            .ToListAsync(ct);
}
