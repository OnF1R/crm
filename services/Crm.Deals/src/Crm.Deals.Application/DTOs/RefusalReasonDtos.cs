namespace Crm.Deals.Application.DTOs;

public record CreateRefusalReasonDto(
    string Name,
    string? Description = null);

public record UpdateRefusalReasonDto(
    string Name,
    string? Description = null,
    bool? IsActive = null);

public record RefusalReasonResponseDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
