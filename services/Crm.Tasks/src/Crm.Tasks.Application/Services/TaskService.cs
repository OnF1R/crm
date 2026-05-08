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
    private readonly ISubTaskRepository _subTaskRepository;
    private readonly ITaskDependencyRepository _taskDependencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TaskService(ITaskRepository taskRepository, ISubTaskRepository subTaskRepository, ITaskDependencyRepository taskDependencyRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _subTaskRepository = subTaskRepository;
        _taskDependencyRepository = taskDependencyRepository;
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

    public async Task<TaskResponseDto> GetByIdWithSubTasksAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetWithCommentsAsync(id, ct)
            ?? throw new KeyNotFoundException("Задача не найдена");
        var subTasks = await _subTaskRepository.GetByParentTaskIdAsync(id, ct);
        return MapToResponse(task, subTasks);
    }

    public async Task<PagedResponse<TaskResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var tasks = await _taskRepository.GetAllAsync(ct);
        var total = tasks.Count;
        var paged = tasks.Skip((page - 1) * pageSize).Take(pageSize).Select(t => MapToResponse(t)).ToList();
        return new PagedResponse<TaskResponseDto>(paged, total, page, pageSize);
    }

    public async Task<PagedResponse<TaskResponseDto>> GetByAssignedUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var tasks = await _taskRepository.GetByAssignedUserIdAsync(userId, ct);
        var total = tasks.Count;
        var paged = tasks.Skip((page - 1) * pageSize).Take(pageSize).Select(t => MapToResponse(t)).ToList();
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

    private static TaskResponseDto MapToResponse(Domain.Entities.TaskItem task, IReadOnlyList<Domain.Entities.SubTask>? subTasks = null) => new(
        task.Id, task.Title, task.Description, task.DueDate,
        task.Priority.ToString(), task.Status.ToString(),
        task.AssignedUserId, task.RelatedEntityType?.ToString(), task.RelatedEntityId, task.CreatedAt,
        subTasks?.Select(MapSubTaskToResponse).ToList());

    private static SubTaskResponseDto MapSubTaskToResponse(Domain.Entities.SubTask st) => new(
        st.Id, st.ParentTaskId, st.Title, st.Description, st.DueDate,
        st.Priority.ToString(), st.Status.ToString(),
        st.AssignedUserId, st.Order, st.CreatedAt,
        st.Status == Domain.Enums.TaskStatus.Завершена);

    public async Task<SubTaskResponseDto> AddSubTaskAsync(Guid parentTaskId, CreateSubTaskDto dto, Guid createdBy, CancellationToken ct = default)
    {
        if (await _taskRepository.GetByIdAsync(parentTaskId, ct) == null)
            throw new KeyNotFoundException("Родительская задача не найдена");

        var subTask = Domain.Entities.SubTask.Create(
            parentTaskId, dto.Title, dto.Description, dto.DueDate,
            (TaskPriority)dto.Priority, dto.AssignedUserId, createdBy, dto.Order);

        await _subTaskRepository.AddAsync(subTask, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapSubTaskToResponse(subTask);
    }

    public async Task<IReadOnlyList<SubTaskResponseDto>> GetSubTasksAsync(Guid parentTaskId, CancellationToken ct = default)
    {
        var parentTask = await _taskRepository.GetByIdAsync(parentTaskId, ct)
            ?? throw new KeyNotFoundException("Родительская задача не найдена");

        var subTasks = await _subTaskRepository.GetByParentTaskIdAsync(parentTaskId, ct);
        return subTasks.Select(MapSubTaskToResponse).ToList();
    }

    public async Task<SubTaskResponseDto> UpdateSubTaskAsync(Guid subTaskId, UpdateSubTaskDto dto, CancellationToken ct = default)
    {
        var subTask = await _subTaskRepository.GetByIdAsync(subTaskId, ct)
            ?? throw new KeyNotFoundException("Подзадача не найдена");

        subTask.Update(dto.Title, dto.Description, dto.DueDate, (TaskPriority)dto.Priority, dto.AssignedUserId, dto.Order);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapSubTaskToResponse(subTask);
    }

    public async Task<SubTaskResponseDto> SetSubTaskStatusAsync(Guid subTaskId, SetTaskStatusDto dto, CancellationToken ct = default)
    {
        var subTask = await _subTaskRepository.GetByIdAsync(subTaskId, ct)
            ?? throw new KeyNotFoundException("Подзадача не найдена");

        subTask.SetStatus((Domain.Enums.TaskStatus)dto.NewStatus);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapSubTaskToResponse(subTask);
    }

    public async Task DeleteSubTaskAsync(Guid subTaskId, CancellationToken ct = default)
    {
        var subTask = await _subTaskRepository.GetByIdAsync(subTaskId, ct)
            ?? throw new KeyNotFoundException("Подзадача не найдена");

        var parentTask = await _taskRepository.GetByIdAsync(subTask.ParentTaskId, ct);
        if (parentTask != null)
        {
            parentTask.RemoveSubTask(subTaskId);
        }

        await _subTaskRepository.DeleteAsync(subTask, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateSubTasksOrderAsync(Guid parentTaskId, CancellationToken ct = default)
    {
        var parentTask = await _taskRepository.GetWithCommentsAsync(parentTaskId, ct)
            ?? throw new KeyNotFoundException("Родительская задача не найдена");

        parentTask.UpdateSubTaskOrder();
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<TaskDependencyResponseDto> AddDependencyAsync(Guid successorTaskId, CreateTaskDependencyDto dto, CancellationToken ct = default)
    {
        if (await _taskRepository.GetByIdAsync(successorTaskId, ct) == null)
            throw new KeyNotFoundException("Задача не найдена");

        var predecessorTask = await _taskRepository.GetByIdAsync(dto.PredecessorTaskId, ct)
            ?? throw new KeyNotFoundException("Предшествующая задача не найдена");

        if (dto.PredecessorTaskId == successorTaskId)
            throw new ArgumentException("Задача не может зависеть от самой себя");

        var dependency = Domain.Entities.TaskDependency.Create(dto.PredecessorTaskId, successorTaskId, (Domain.Entities.DependencyType)dto.Type);
        await _taskDependencyRepository.AddAsync(dependency, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapDependencyToResponse(dependency);
    }

    public async Task<IReadOnlyList<TaskDependencyResponseDto>> GetDependenciesAsync(Guid taskId, CancellationToken ct = default)
    {
        var dependencies = await _taskDependencyRepository.GetByTaskIdAsync(taskId, ct);
        return dependencies.Select(MapDependencyToResponse).ToList();
    }

    public async Task DeleteDependencyAsync(Guid dependencyId, CancellationToken ct = default)
    {
        var dependency = await _taskDependencyRepository.GetByIdAsync(dependencyId, ct)
            ?? throw new KeyNotFoundException("Зависимость не найдена");

        var task = await _taskRepository.GetByIdAsync(dependency.SuccessorTaskId, ct);
        if (task != null)
        {
            task.RemoveDependency(dependencyId);
        }

        await _taskDependencyRepository.DeleteAsync(dependency, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static TaskDependencyResponseDto MapDependencyToResponse(Domain.Entities.TaskDependency d) => new(
        d.Id, d.PredecessorTaskId, d.SuccessorTaskId,
        d.Type.ToString(), d.CreatedAt);
}
