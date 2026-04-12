using Crm.Shared.Domain;

namespace Crm.Clients.Domain.Entities;

public class Contact : Entity, IAuditable
{
    public Guid ClientId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    private Contact() { }

    public static Contact Create(Guid clientId, string firstName, string lastName, string email, string? phone, string? position, bool isPrimary)
    {
        return new Contact
        {
            ClientId = clientId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Position = position,
            IsPrimary = isPrimary,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string firstName, string lastName, string email, string? phone, string? position, bool isPrimary)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Position = position;
        IsPrimary = isPrimary;
    }

    public string GetFullName() => $"{FirstName} {LastName}";
}
