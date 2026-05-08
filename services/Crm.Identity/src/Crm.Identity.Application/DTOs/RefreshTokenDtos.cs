namespace Crm.Identity.Application.DTOs;

public record RefreshTokenDto(
    Guid Id,
    string Token,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    DateTime? RevokedAt,
    string? RevokedReason,
    string? IpAddress);

public record RefreshRequestDto(
    string RefreshToken);

public record RefreshResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
