namespace Crm.Documents.Application.DTOs;

public record DocumentResponseDto(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    Guid UploadedByUserId,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    DateTime CreatedAt);
