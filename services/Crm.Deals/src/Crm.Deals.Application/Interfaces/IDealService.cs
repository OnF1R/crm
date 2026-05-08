using Crm.Deals.Application.DTOs;
using Crm.Shared.DTOs;

namespace Crm.Deals.Application.Interfaces;

public interface IDealService
{
    Task<DealResponseDto> CreateAsync(CreateDealDto dto, Guid createdBy, CancellationToken ct = default);
    Task<DealResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResponse<DealResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedResponse<DealResponseDto>> GetByClientIdAsync(Guid clientId, int page, int pageSize, CancellationToken ct = default);
    Task<DealResponseDto> UpdateAsync(Guid id, UpdateDealDto dto, CancellationToken ct = default);
    Task<DealResponseDto> MoveStageAsync(Guid id, MoveDealStageDto dto, Guid movedBy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // Refusal reason methods
    Task<RefusalReasonResponseDto> CreateRefusalReasonAsync(CreateRefusalReasonDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<RefusalReasonResponseDto>> GetAllRefusalReasonsAsync(CancellationToken ct = default);
    Task<RefusalReasonResponseDto> GetRefusalReasonByIdAsync(Guid id, CancellationToken ct = default);
    Task<RefusalReasonResponseDto> UpdateRefusalReasonAsync(Guid id, UpdateRefusalReasonDto dto, CancellationToken ct = default);
    Task DeleteRefusalReasonAsync(Guid id, CancellationToken ct = default);
    Task SetDealRefusalReasonAsync(Guid dealId, Guid? refusalReasonId, CancellationToken ct = default);
}
