using Crm.Deals.Domain.Enums;
using Crm.Shared.Domain;

namespace Crm.Deals.Domain.Entities;

public class Deal : AggregateRoot, IAuditable, ISoftDeletable
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public DealStage Stage { get; set; }
    public int Probability { get; set; }
    public Guid ClientId { get; set; }
    public Guid AssignedUserId { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? RefusalReasonId { get; set; }

    private readonly List<DealStageHistory> _stageHistory = [];
    public IReadOnlyList<DealStageHistory> StageHistory => _stageHistory.AsReadOnly();

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    private Deal() { }

    public static Deal Create(string title, string? description, decimal amount, Currency currency, DealStage stage, Guid clientId, Guid assignedUserId, Guid createdBy)
    {
        var deal = new Deal
        {
            Title = title,
            Description = description,
            Amount = amount,
            Currency = currency,
            Stage = stage,
            Probability = GetDefaultProbabilityPublic(stage),
            ClientId = clientId,
            AssignedUserId = assignedUserId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
        deal._stageHistory.Add(DealStageHistory.Create(deal.Id, stage, createdBy));
        return deal;
    }

    public void MoveToStage(DealStage newStage, Guid changedBy)
    {
        Stage = newStage;
        Probability = GetDefaultProbabilityPublic(newStage);
        _stageHistory.Add(DealStageHistory.Create(Id, newStage, changedBy));

        if (newStage is DealStage.ЗакрытиеУспех or DealStage.ЗакрытиеПровал)
            ClosedAt = DateTime.UtcNow;
    }

    public void SetRefusalReason(Guid? refusalReasonId)
    {
        RefusalReasonId = refusalReasonId;
    }

    public void Update(string title, string? description, decimal amount, Currency currency, Guid assignedUserId)
    {
        Title = title;
        Description = description;
        Amount = amount;
        Currency = currency;
        AssignedUserId = assignedUserId;
    }

    public static int GetDefaultProbabilityPublic(DealStage stage) => stage switch
    {
        DealStage.Квалификация => 10,
        DealStage.Презентация => 25,
        DealStage.Переговоры => 50,
        DealStage.Договор => 75,
        DealStage.ЗакрытиеУспех => 100,
        DealStage.ЗакрытиеПровал => 0,
        _ => 0
    };
}
