using Crm.Shared.Domain;

namespace Crm.Documents.Domain.Entities;

public class Document : AggregateRoot
{
    public string FileName { get; set; } = null!;
    public string? StoredFileName { get; set; }
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }

    private Document() { }

    public static Document Create(string fileName, string contentType, long size, Guid uploadedByUserId, string? relatedEntityType = null, Guid? relatedEntityId = null)
    {
        return new Document
        {
            FileName = fileName,
            ContentType = contentType,
            Size = size,
            UploadedByUserId = uploadedByUserId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
