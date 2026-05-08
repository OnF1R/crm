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

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

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

    public RefreshToken AddRefreshToken(string token, DateTime expiresAt, string? ipAddress = null, string? userAgent = null)
    {
        var refreshToken = RefreshToken.Create(Id, token, expiresAt, ipAddress, userAgent);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeRefreshToken(string token, string? reason = null, string? replacedBy = null)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
        if (refreshToken != null)
        {
            refreshToken.Revoke(reason, replacedBy);
        }
    }

    public void RevokeAllRefreshTokens(string? reason = null)
    {
        foreach (var refreshToken in _refreshTokens.Where(rt => rt.IsActive))
        {
            refreshToken.Revoke(reason);
        }
    }

    public RefreshToken? GetActiveRefreshToken()
    {
        return _refreshTokens.FirstOrDefault(rt => rt.IsActive);
    }

    public string GetFullName() => $"{FirstName} {LastName}";
}
