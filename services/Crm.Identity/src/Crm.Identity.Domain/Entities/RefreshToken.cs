using Crm.Shared.Domain;

namespace Crm.Identity.Domain.Entities;

public class RefreshToken : AggregateRoot
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt, string? ipAddress = null, string? userAgent = null)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;

    public void Revoke(string? reason = null, string? replacedBy = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        ReplacedByToken = replacedBy;
    }
}
