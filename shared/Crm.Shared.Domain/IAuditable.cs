namespace Crm.Shared.Domain;

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    Guid CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}
