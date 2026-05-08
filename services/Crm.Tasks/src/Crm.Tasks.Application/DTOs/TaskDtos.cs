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
    DateTime CreatedAt,
    IReadOnlyList<SubTaskResponseDto>? SubTasks = null);

public record CreateCommentDto(string Content);

public record CommentResponseDto(
    Guid Id,
    Guid TaskId,
    string Content,
    Guid AuthorUserId,
    DateTime CreatedAt);

public record CreateSubTaskDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    int Priority,
    Guid AssignedUserId,
    int Order = 0);

public record UpdateSubTaskDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    int Priority,
    Guid AssignedUserId,
    int Order);

public record SubTaskResponseDto(
    Guid Id,
    Guid ParentTaskId,
    string Title,
    string? Description,
    DateTime? DueDate,
    string Priority,
    string Status,
    Guid AssignedUserId,
    int Order,
    DateTime CreatedAt,
    bool IsCompleted = false);

public record CreateTaskDependencyDto(
    Guid PredecessorTaskId,
    int Type);

public record TaskDependencyResponseDto(
    Guid Id,
    Guid PredecessorTaskId,
    Guid SuccessorTaskId,
    string Type,
    DateTime CreatedAt);
