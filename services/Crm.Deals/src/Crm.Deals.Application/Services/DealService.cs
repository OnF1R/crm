using Crm.Deals.Application.DTOs;
using Crm.Deals.Application.Interfaces;
using Crm.Deals.Domain.Enums;
using Crm.Deals.Domain.Interfaces;
using Crm.Shared.Domain;
using Crm.Shared.DTOs;

namespace Crm.Deals.Application.Services;

public class DealService : IDealService
{
    private readonly IDealRepository _dealRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DealService(IDealRepository dealRepository, IUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DealResponseDto> CreateAsync(CreateDealDto dto, Guid createdBy, CancellationToken ct = default)
    {
        var deal = Domain.Entities.Deal.Create(dto.Title, dto.Description, dto.Amount, (Currency)dto.Currency, (DealStage)dto.Stage, dto.ClientId, dto.AssignedUserId, createdBy);
        await _dealRepository.AddAsync(deal, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(deal);
    }

    public async Task<DealResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var deal = await _dealRepository.GetWithHistoryAsync(id, ct)
            ?? throw new KeyNotFoundException("Сделка не найдена");
        return MapToResponse(deal);
    }

    public async Task<PagedResponse<DealResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var deals = await _dealRepository.GetAllAsync(ct);
        var total = deals.Count;
        var paged = deals.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList();
        return new PagedResponse<DealResponseDto>(paged, total, page, pageSize);
    }

    public async Task<PagedResponse<DealResponseDto>> GetByClientIdAsync(Guid clientId, int page, int pageSize, CancellationToken ct = default)
    {
        var deals = await _dealRepository.GetByClientIdAsync(clientId, ct);
        var total = deals.Count;
        var paged = deals.Skip((page - 1) * pageSize).Take(pageSize).Select(MapToResponse).ToList();
        return new PagedResponse<DealResponseDto>(paged, total, page, pageSize);
    }

    public async Task<DealResponseDto> UpdateAsync(Guid id, UpdateDealDto dto, CancellationToken ct = default)
    {
        var deal = await _dealRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Сделка не найдена");
        deal.Update(dto.Title, dto.Description, dto.Amount, (Currency)dto.Currency, dto.AssignedUserId);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(deal);
    }

    public async Task<DealResponseDto> MoveStageAsync(Guid id, MoveDealStageDto dto, Guid movedBy, CancellationToken ct = default)
    {
        var deal = await _dealRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Сделка не найдена");
        var newStage = (DealStage)dto.NewStage;
        deal.Stage = newStage;
        deal.Probability = Domain.Entities.Deal.GetDefaultProbabilityPublic(newStage);
        if (newStage is DealStage.ЗакрытиеУспех or DealStage.ЗакрытиеПровал)
            deal.ClosedAt = DateTime.UtcNow;
        var history = Domain.Entities.DealStageHistory.Create(id, newStage, movedBy);
        await _dealRepository.AddStageHistoryAsync(history, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(deal);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var deal = await _dealRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Сделка не найдена");
        await _dealRepository.DeleteAsync(deal, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static DealResponseDto MapToResponse(Domain.Entities.Deal deal) => new(
        deal.Id, deal.Title, deal.Description, deal.Amount, deal.Currency.ToString(),
        deal.Stage.ToString(), deal.Probability, deal.ClientId, deal.AssignedUserId,
        deal.ClosedAt, deal.CreatedAt,
        deal.StageHistory.Select(h => new DealStageHistoryDto(h.Id, h.Stage.ToString(), h.ChangedAt, h.ChangedByUserId)).ToList());
}
