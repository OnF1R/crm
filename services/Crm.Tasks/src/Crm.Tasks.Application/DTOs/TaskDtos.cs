namespace Crm.Tasks.Application.DTOs;

public record CreateTaskDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    int Priority,
    Guid AssignedUserId,
    int? RelatedEntityType = null,
    Guid? RelatedEntityId = null);

public record UpdateTaskDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    int Priority,
    Guid AssignedUserId);

public record SetTaskStatusDto(int NewStatus);

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

public record CreateCommentDto(string Content);

public record CommentResponseDto(
    Guid Id,
    Guid TaskId,
    string Content,
    Guid AuthorUserId,
    DateTime CreatedAt);
