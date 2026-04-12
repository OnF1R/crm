using Crm.Tasks.Application.DTOs;
using Crm.Shared.DTOs;

namespace Crm.Tasks.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto, Guid createdBy, CancellationToken ct = default);
    Task<TaskResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResponse<TaskResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedResponse<TaskResponseDto>> GetByAssignedUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<TaskResponseDto> UpdateAsync(Guid id, UpdateTaskDto dto, CancellationToken ct = default);
    Task<TaskResponseDto> SetStatusAsync(Guid id, SetTaskStatusDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<CommentResponseDto> AddCommentAsync(Guid taskId, CreateCommentDto dto, Guid authorUserId, CancellationToken ct = default);
    Task<IReadOnlyList<CommentResponseDto>> GetCommentsAsync(Guid taskId, CancellationToken ct = default);
}
