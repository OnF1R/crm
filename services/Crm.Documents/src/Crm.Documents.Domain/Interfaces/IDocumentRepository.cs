using Crm.Documents.Domain.Entities;
using Crm.Shared.Domain;

namespace Crm.Documents.Domain.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IReadOnlyList<Document>> GetByRelatedEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
}
