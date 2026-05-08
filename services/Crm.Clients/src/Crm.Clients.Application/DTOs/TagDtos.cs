namespace Crm.Clients.Application.DTOs;

public record CreateTagDto(
    string Name,
    string? Description = null,
    string? Color = null);

public record UpdateTagDto(
    string Name,
    string? Description = null,
    string? Color = null);

public record TagResponseDto(
    Guid Id,
    string Name,
    string? Description,
    string? Color,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
