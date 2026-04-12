namespace Crm.Deals.Application.DTOs;

public record CreateDealDto(
    string Title,
    string? Description,
    decimal Amount,
    int Currency,
    int Stage,
    Guid ClientId,
    Guid AssignedUserId);

public record UpdateDealDto(
    string Title,
    string? Description,
    decimal Amount,
    int Currency,
    Guid AssignedUserId);

public record MoveDealStageDto(int NewStage);

public record DealResponseDto(
    Guid Id,
    string Title,
    string? Description,
    decimal Amount,
    string Currency,
    string Stage,
    int Probability,
    Guid ClientId,
    Guid AssignedUserId,
    DateTime? ClosedAt,
    DateTime CreatedAt,
    IReadOnlyList<DealStageHistoryDto> StageHistory);

public record DealStageHistoryDto(
    Guid Id,
    string Stage,
    DateTime ChangedAt,
    Guid ChangedByUserId);
