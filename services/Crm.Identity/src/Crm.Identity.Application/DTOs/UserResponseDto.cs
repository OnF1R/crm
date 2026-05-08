namespace Crm.Identity.Application.DTOs;

public record UserResponseDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    int Role,
    string RoleName,
    string Status,
    DateTime CreatedAt);

public record UpdateUserDto(
    string FirstName,
    string LastName,
    int Role);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword);

public record TokenResponseDto(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    UserResponseDto User,
    string? RefreshToken = null);
