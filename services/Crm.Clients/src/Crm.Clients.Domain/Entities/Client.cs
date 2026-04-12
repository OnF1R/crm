using Crm.Clients.Domain.Enums;
using Crm.Shared.Domain;

namespace Crm.Clients.Domain.Entities;

public class Client : AggregateRoot, IAuditable, ISoftDeletable
{
    public string CompanyName { get; set; } = null!;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Inn { get; set; }
    public string? Website { get; set; }
    public Industry Industry { get; set; }
    public ClientStatus Status { get; set; }
    public Guid AssignedUserId { get; set; }

    private readonly List<Contact> _contacts = [];
    public IReadOnlyList<Contact> Contacts => _contacts.AsReadOnly();

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    private Client() { }

    public static Client Create(string companyName, string? description, string? avatarUrl, string? inn, string? website, Industry industry, Guid assignedUserId, Guid createdBy)
    {
        return new Client
        {
            CompanyName = companyName,
            Description = description,
            AvatarUrl = avatarUrl,
            Inn = inn,
            Website = website,
            Industry = industry,
            Status = ClientStatus.Новый,
            AssignedUserId = assignedUserId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string companyName, string? description, string? avatarUrl, string? inn, string? website, Industry industry, Guid assignedUserId)
    {
        CompanyName = companyName;
        Description = description;
        AvatarUrl = avatarUrl;
        Inn = inn;
        Website = website;
        Industry = industry;
        AssignedUserId = assignedUserId;
    }

    public void SetStatus(ClientStatus status) => Status = status;

    public Contact AddContact(string firstName, string lastName, string email, string? phone, string? position, bool isPrimary)
    {
        var contact = Contact.Create(Id, firstName, lastName, email, phone, position, isPrimary);
        _contacts.Add(contact);
        return contact;
    }
}
