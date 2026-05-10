using Crm.Deals.Domain.Entities;
using Crm.Deals.Domain.Enums;
using Crm.Shared.Domain;
using Xunit;

namespace Crm.Deals.Tests;

public sealed class DealDomainTests
{
    private static readonly Guid ClientId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AssignedUserId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid CreatedBy = new("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ChangedBy = new("44444444-4444-4444-4444-444444444444");

    [Fact]
    public void Create_ShouldInitializeMandatoryFieldsAndHistory()
    {
        var startedAt = DateTime.UtcNow;
        var deal = Deal.Create(
            "Test deal",
            "Test description",
            150m,
            (Currency)1,
            (DealStage)1,
            ClientId,
            AssignedUserId,
            CreatedBy);

        Assert.Equal("Test deal", deal.Title);
        Assert.Equal("Test description", deal.Description);
        Assert.Equal(150m, deal.Amount);
        Assert.Equal((Currency)1, deal.Currency);
        Assert.Equal((DealStage)1, deal.Stage);
        Assert.Equal(10, deal.Probability);
        Assert.Equal(ClientId, deal.ClientId);
        Assert.Equal(AssignedUserId, deal.AssignedUserId);
        Assert.Equal(CreatedBy, deal.CreatedBy);
        Assert.InRange(deal.CreatedAt, startedAt, DateTime.UtcNow);
        Assert.Single(deal.StageHistory);
        Assert.Equal(deal.Id, deal.StageHistory[0].DealId);
        Assert.Equal((DealStage)1, deal.StageHistory[0].Stage);
        Assert.Equal(CreatedBy, deal.StageHistory[0].ChangedByUserId);
    }

    [Theory]
    [InlineData(2, 25)]
    [InlineData(5, 100)]
    [InlineData(6, 0)]
    public void MoveToStage_ShouldUpdateProbabilityAndAddHistory(int stage, int expectedProbability)
    {
        var deal = Deal.Create(
            "Test deal",
            null,
            100m,
            (Currency)2,
            (DealStage)1,
            ClientId,
            AssignedUserId,
            CreatedBy);
        var beforeMove = DateTime.UtcNow;

        deal.MoveToStage((DealStage)stage, ChangedBy);

        Assert.Equal((DealStage)stage, deal.Stage);
        Assert.Equal(expectedProbability, deal.Probability);
        Assert.Equal(2, deal.StageHistory.Count);
        Assert.Equal((DealStage)stage, deal.StageHistory[1].Stage);
        Assert.Equal(ChangedBy, deal.StageHistory[1].ChangedByUserId);

        if (stage == 5 || stage == 6)
        {
            Assert.NotNull(deal.ClosedAt);
            Assert.InRange(deal.ClosedAt.Value, beforeMove, DateTime.UtcNow);
        }
        else
        {
            Assert.Null(deal.ClosedAt);
        }
    }

    [Fact]
    public void Update_ShouldOnlyChangeMutableFields()
    {
        var deal = Deal.Create("Old title", "Old desc", 10m, (Currency)1, (DealStage)1, ClientId, AssignedUserId, CreatedBy);
        var existingStage = deal.Stage;

        deal.Update("New title", null, 20m, (Currency)2, AssignedUserId);

        Assert.Equal("New title", deal.Title);
        Assert.Null(deal.Description);
        Assert.Equal(20m, deal.Amount);
        Assert.Equal((Currency)2, deal.Currency);
        Assert.Equal(AssignedUserId, deal.AssignedUserId);
        Assert.Equal(existingStage, deal.Stage);
        Assert.NotEqual(10m, deal.Amount);
    }

    [Fact]
    public void Update_ShouldNotClearRefusalReasonOrChangeStage()
    {
        var existingReason = Guid.NewGuid();
        var deal = Deal.Create("Deal", "Desc", 10m, (Currency)3, (DealStage)2, ClientId, AssignedUserId, CreatedBy);
        deal.SetRefusalReason(existingReason);

        deal.Update("Deal updated", "Updated", 50m, (Currency)1, AssignedUserId);

        Assert.Equal((DealStage)2, deal.Stage);
        Assert.Equal(existingReason, deal.RefusalReasonId);
    }

    [Fact]
    public void SetRefusalReason_ShouldSetAndReplaceValue()
    {
        var reason = Guid.NewGuid();
        var reasonTwo = Guid.NewGuid();
        var deal = Deal.Create("Deal", null, 1m, (Currency)1, (DealStage)1, ClientId, AssignedUserId, CreatedBy);

        deal.SetRefusalReason(reason);
        Assert.Equal(reason, deal.RefusalReasonId);

        deal.SetRefusalReason(reasonTwo);
        Assert.Equal(reasonTwo, deal.RefusalReasonId);
    }
}

public sealed class RefusalReasonTests
{
    [Fact]
    public void Create_ShouldSetDefaults()
    {
        var reason = RefusalReason.Create("No budget", "Customer has no budget");
        var now = DateTime.UtcNow;

        Assert.Equal("No budget", reason.Name);
        Assert.Equal("Customer has no budget", reason.Description);
        Assert.True(reason.IsActive);
        Assert.InRange(reason.CreatedAt, now.AddSeconds(-1), DateTime.UtcNow);
        Assert.False(reason.UpdatedAt.HasValue);
    }

    [Fact]
    public void Update_ShouldReplaceFieldsAndTouchUpdatedAt()
    {
        var reason = RefusalReason.Create("No budget");
        var beforeUpdate = DateTime.UtcNow;

        reason.Update("Needs review", "Changed reason");

        Assert.Equal("Needs review", reason.Name);
        Assert.Equal("Changed reason", reason.Description);
        Assert.InRange(reason.UpdatedAt!.Value, beforeUpdate, DateTime.UtcNow);
    }

    [Fact]
    public void SetActive_ShouldToggleStateAndTouchUpdatedAt()
    {
        var reason = RefusalReason.Create("No budget");
        var before = DateTime.UtcNow;

        reason.SetActive(false);

        Assert.False(reason.IsActive);
        Assert.InRange(reason.UpdatedAt!.Value, before, DateTime.UtcNow);
    }
}

public sealed class AggregateRootTests
{
    [Fact]
    public void AggregateRoot_ShouldTrackAndClearDomainEvents()
    {
        var aggregate = new StubAggregateRoot();
        aggregate.TrackEvent(new TestDomainEvent());

        Assert.Single(aggregate.DomainEvents);

        aggregate.ClearDomainEvents();
        Assert.Empty(aggregate.DomainEvents);
    }

    private sealed class StubAggregateRoot : AggregateRoot
    {
        public void TrackEvent(DomainEvent @event) => AddDomainEvent(@event);
    }

    private sealed record TestDomainEvent : DomainEvent { }
}
