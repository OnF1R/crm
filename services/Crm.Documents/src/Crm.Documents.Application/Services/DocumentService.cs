using Crm.Documents.Application.DTOs;
using Crm.Documents.Application.Interfaces;
using Crm.Documents.Domain.Interfaces;
using Crm.Shared.Domain;
using Crm.Shared.DTOs;

namespace Crm.Documents.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(IDocumentRepository documentRepository, IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentResponseDto> UploadAsync(Stream fileStream, string fileName, string contentType, Guid uploadedByUserId, string? relatedEntityType = null, Guid? relatedEntityId = null, CancellationToken ct = default)
    {
        var size = fileStream.Length;
        var storedFileName = $"{Guid.NewGuid()}_{fileName}";
        await _fileStorageService.SaveAsync(fileStream, storedFileName);
        var document = Domain.Entities.Document.Create(fileName, contentType, size, uploadedByUserId, relatedEntityType, relatedEntityId);
        document.GetType().GetProperty("StoredFileName")?.SetValue(document, storedFileName);
        await _documentRepository.AddAsync(document, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(document);
    }

    public async Task<DocumentResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Документ не найден");
        return MapToResponse(document);
    }

    public async Task<PagedResponse<DocumentResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var documents = await _documentRepository.GetAllAsync(ct);
        var total = documents.Count;
        var paged = documents.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList();
        return new PagedResponse<DocumentResponseDto>(paged, total, page, pageSize);
    }

    public async Task<PagedResponse<DocumentResponseDto>> GetByRelatedEntityAsync(string entityType, Guid entityId, int page, int pageSize, CancellationToken ct = default)
    {
        var documents = await _documentRepository.GetByRelatedEntityAsync(entityType, entityId, ct);
        var total = documents.Count;
        var paged = documents.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList();
        return new PagedResponse<DocumentResponseDto>(paged, total, page, pageSize);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(Guid id, CancellationToken ct = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Документ не найден");
        var storedFileName = (string?)document.GetType().GetProperty("StoredFileName")?.GetValue(document)! ?? document.FileName;
        var stream = await _fileStorageService.GetAsync(storedFileName)
            ?? throw new FileNotFoundException("Файл не найден на диске");
        return (stream, document.ContentType, document.FileName);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Документ не найден");
        var storedFileName = (string?)document.GetType().GetProperty("StoredFileName")?.GetValue(document)! ?? document.FileName;
        await _fileStorageService.DeleteAsync(storedFileName);
        await _documentRepository.DeleteAsync(document, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static DocumentResponseDto MapToResponse(Domain.Entities.Document d) => new(
        d.Id, d.FileName, d.ContentType, d.Size, d.UploadedByUserId, d.RelatedEntityType, d.RelatedEntityId, d.CreatedAt);
}
