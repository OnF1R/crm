namespace Crm.Clients.Application.DTOs;

public record CreateClientDto(
    string CompanyName,
    string? Description,
    string? AvatarUrl,
    string? Inn,
    string? Website,
    int Industry,
    Guid AssignedUserId);

public record UpdateClientDto(
    string CompanyName,
    string? Description,
    string? AvatarUrl,
    string? Inn,
    string? Website,
    int Industry,
    Guid AssignedUserId);

public record ClientResponseDto(
    Guid Id,
    string CompanyName,
    string? Description,
    string? AvatarUrl,
    string? Inn,
    string? Website,
    string Industry,
    string Status,
    Guid AssignedUserId,
    DateTime CreatedAt,
    IReadOnlyList<ContactResponseDto> Contacts);

public record CreateContactDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Position,
    bool IsPrimary);

public record UpdateContactDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Position,
    bool IsPrimary);

public record ContactResponseDto(
    Guid Id,
    Guid ClientId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    string? Position,
    bool IsPrimary);
