using Crm.Deals.Domain.Enums;
using Crm.Shared.Domain;

namespace Crm.Deals.Domain.Entities;

public class DealStageHistory : Entity
{
    public Guid DealId { get; set; }
    public DealStage Stage { get; set; }
    public DateTime ChangedAt { get; set; }
    public Guid ChangedByUserId { get; set; }

    private DealStageHistory() { }

    public static DealStageHistory Create(Guid dealId, DealStage stage, Guid changedByUserId)
    {
        return new DealStageHistory
        {
            DealId = dealId,
            Stage = stage,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId
        };
    }
}
