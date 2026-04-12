using Crm.Tasks.Domain.Enums;
using TaskStatus = Crm.Tasks.Domain.Enums.TaskStatus;

namespace Crm.Tasks.Application.DTOs;

public record CreateTaskDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    TaskPriority Priority,
    Guid AssignedUserId,
    RelatedEntityType? RelatedEntityType = null,
    Guid? RelatedEntityId = null);

public record UpdateTaskDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    TaskPriority Priority,
    Guid AssignedUserId);

public record SetTaskStatusDto(TaskStatus NewStatus);

public record TaskResponseDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    string Priority,
    string Status,
    Guid AssignedUserId,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    DateTime CreatedAt);
