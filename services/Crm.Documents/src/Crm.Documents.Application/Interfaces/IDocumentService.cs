using Crm.Documents.Application.DTOs;
using Crm.Shared.DTOs;

namespace Crm.Documents.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponseDto> UploadAsync(Stream fileStream, string fileName, string contentType, Guid uploadedByUserId, string? relatedEntityType = null, Guid? relatedEntityId = null, CancellationToken ct = default);
    Task<DocumentResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResponse<DocumentResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedResponse<DocumentResponseDto>> GetByRelatedEntityAsync(string entityType, Guid entityId, int page, int pageSize, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
