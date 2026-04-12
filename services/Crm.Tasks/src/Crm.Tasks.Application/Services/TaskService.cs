using Crm.Tasks.Application.DTOs;
using Crm.Tasks.Application.Interfaces;
using Crm.Tasks.Domain.Enums;
using Crm.Tasks.Domain.Interfaces;
using Crm.Shared.Domain;
using Crm.Shared.DTOs;

namespace Crm.Tasks.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TaskService(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto, Guid createdBy, CancellationToken ct = default)
    {
        var task = Domain.Entities.TaskItem.Create(
            dto.Title, dto.Description, dto.DueDate,
            (TaskPriority)dto.Priority, dto.AssignedUserId, createdBy,
            dto.RelatedEntityType.HasValue ? (RelatedEntityType?)dto.RelatedEntityType.Value : null,
            dto.RelatedEntityId);
        await _taskRepository.AddAsync(task, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(task);
    }

    public async Task<TaskResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Задача не найдена");
        return MapToResponse(task);
    }

    public async Task<PagedResponse<TaskResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var tasks = await _taskRepository.GetAllAsync(ct);
        var total = tasks.Count;
        var paged = tasks.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList();
        return new PagedResponse<TaskResponseDto>(paged, total, page, pageSize);
    }

    public async Task<PagedResponse<TaskResponseDto>> GetByAssignedUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var tasks = await _taskRepository.GetByAssignedUserIdAsync(userId, ct);
        var total = tasks.Count;
        var paged = tasks.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList();
        return new PagedResponse<TaskResponseDto>(paged, total, page, pageSize);
    }

    public async Task<TaskResponseDto> UpdateAsync(Guid id, UpdateTaskDto dto, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Задача не найдена");
        task.Update(dto.Title, dto.Description, dto.DueDate, (TaskPriority)dto.Priority, dto.AssignedUserId);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(task);
    }

    public async Task<TaskResponseDto> SetStatusAsync(Guid id, SetTaskStatusDto dto, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Задача не найдена");
        task.SetStatus((Domain.Enums.TaskStatus)dto.NewStatus);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(task);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Задача не найдена");
        await _taskRepository.DeleteAsync(task, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<CommentResponseDto> AddCommentAsync(Guid taskId, CreateCommentDto dto, Guid authorUserId, CancellationToken ct = default)
    {
        var taskExists = await _taskRepository.GetByIdAsync(taskId, ct) != null;
        if (!taskExists) throw new KeyNotFoundException("Задача не найдена");
        var comment = Domain.Entities.TaskComment.Create(taskId, dto.Content, authorUserId);
        await _taskRepository.AddCommentAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapCommentToResponse(comment);
    }

    public async Task<IReadOnlyList<CommentResponseDto>> GetCommentsAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetWithCommentsAsync(taskId, ct)
            ?? throw new KeyNotFoundException("Задача не найдена");
        return task.Comments.Select(MapCommentToResponse).ToList();
    }

    private static CommentResponseDto MapCommentToResponse(Domain.Entities.TaskComment c) => new(c.Id, c.TaskId, c.Content, c.AuthorUserId, c.CreatedAt);

    private static TaskResponseDto MapToResponse(Domain.Entities.TaskItem task) => new(
        task.Id, task.Title, task.Description, task.DueDate,
        task.Priority.ToString(), task.Status.ToString(),
        task.AssignedUserId, task.RelatedEntityType?.ToString(), task.RelatedEntityId, task.CreatedAt);
}
