using Crm.Identity.Domain.Enums;
using Crm.Shared.Domain;

namespace Crm.Identity.Domain.Entities;

public class User : AggregateRoot, IAuditable, ISoftDeletable
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, string lastName, UserRole role, Guid createdBy)
    {
        return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            Status = UserStatus.Активен,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string firstName, string lastName, UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Role = role;
    }

    public void SetStatus(UserStatus status)
    {
        Status = status;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public string GetFullName() => $"{FirstName} {LastName}";
}
