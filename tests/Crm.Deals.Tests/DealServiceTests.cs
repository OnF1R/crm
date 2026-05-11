using Crm.Deals.Application.DTOs;
using Crm.Deals.Application.Infrastructure.Ports;
using Crm.Deals.Application.Services;
using Crm.Deals.Domain.Entities;
using Crm.Deals.Domain.Interfaces;
using Crm.Shared.Domain;
using Xunit;

namespace Crm.Deals.Tests;

public sealed class DealServiceTests
{
    private static readonly Guid ClientId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AssignedUserId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid MovedBy = new("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task MoveStageAsync_ShouldCallClientStatusUpdaterForClosedWon()
    {
        var deal = Deal.Create("Deal", "Desc", 100m, (Domain.Enums.Currency)1, (Domain.Enums.DealStage)1, ClientId, AssignedUserId, MovedBy);
        var dealRepo = new InMemoryDealRepository(deal);
        var refusalRepo = new InMemoryRefusalReasonRepository();
        var updater = new FakeClientStatusUpdater();
        var uow = new FakeUnitOfWork();
        var service = new DealService(dealRepo, refusalRepo, uow, updater);

        await service.MoveStageAsync(deal.Id, new MoveDealStageDto(5), MovedBy);

        Assert.Equal((Domain.Enums.DealStage)5, deal.Stage);
        Assert.True(deal.ClosedAt.HasValue);
        Assert.Equal(1, updater.ClosedCalls);
        Assert.Equal(ClientId, updater.ClosedClientId);
        Assert.Equal(0, updater.ActiveCalls);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task MoveStageAsync_ShouldSetClientActiveForClosedLost()
    {
        var deal = Deal.Create("Deal", "Desc", 100m, (Domain.Enums.Currency)1, (Domain.Enums.DealStage)1, ClientId, AssignedUserId, MovedBy);
        var dealRepo = new InMemoryDealRepository(deal);
        var refusalRepo = new InMemoryRefusalReasonRepository();
        var updater = new FakeClientStatusUpdater();
        var uow = new FakeUnitOfWork();
        var service = new DealService(dealRepo, refusalRepo, uow, updater);

        await service.MoveStageAsync(deal.Id, new MoveDealStageDto(6), MovedBy);

        Assert.Equal((Domain.Enums.DealStage)6, deal.Stage);
        Assert.True(deal.ClosedAt.HasValue);
        Assert.Equal(0, updater.ClosedCalls);
        Assert.Null(updater.ClosedClientId);
        Assert.Equal(1, updater.ActiveCalls);
        Assert.Equal(ClientId, updater.ActiveClientId);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task MoveStageAsync_ShouldSetClientActiveAndClearClosedAtWhenReopened()
    {
        var deal = Deal.Create("Deal", "Desc", 100m, (Domain.Enums.Currency)1, (Domain.Enums.DealStage)5, ClientId, AssignedUserId, MovedBy);
        deal.ClosedAt = DateTime.UtcNow.AddDays(-1);
        var dealRepo = new InMemoryDealRepository(deal);
        var refusalRepo = new InMemoryRefusalReasonRepository();
        var updater = new FakeClientStatusUpdater();
        var uow = new FakeUnitOfWork();
        var service = new DealService(dealRepo, refusalRepo, uow, updater);

        await service.MoveStageAsync(deal.Id, new MoveDealStageDto(1), MovedBy);

        Assert.Equal((Domain.Enums.DealStage)1, deal.Stage);
        Assert.Null(deal.ClosedAt);
        Assert.Equal(0, updater.ClosedCalls);
        Assert.Equal(1, updater.ActiveCalls);
        Assert.Equal(ClientId, updater.ActiveClientId);
        Assert.Equal(1, uow.SaveCount);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeClientStatusUpdater : IClientStatusUpdater
    {
        public int ClosedCalls { get; private set; }
        public int ActiveCalls { get; private set; }
        public Guid? ClosedClientId { get; private set; }
        public Guid? ActiveClientId { get; private set; }

        public Task SetClosedAsync(Guid clientId, CancellationToken ct = default)
        {
            ClosedCalls++;
            ClosedClientId = clientId;
            return Task.CompletedTask;
        }

        public Task SetActiveAsync(Guid clientId, CancellationToken ct = default)
        {
            ActiveCalls++;
            ActiveClientId = clientId;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryDealRepository : IDealRepository
    {
        private readonly List<Deal> _deals;

        public InMemoryDealRepository(params Deal[] deals)
        {
            _deals = deals.ToList();
        }

        public Task<IReadOnlyList<Deal>> FindAsync(ISpecification<Deal> specification, CancellationToken ct = default)
        {
            return Task.FromResult((IReadOnlyList<Deal>)_deals.ToList());
        }

        public Task<Deal?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_deals.FirstOrDefault(d => d.Id == id));

        public Task<IReadOnlyList<Deal>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Deal>)_deals.ToList());

        public Task<IReadOnlyList<Deal>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Deal>)_deals.Where(d => d.ClientId == clientId).ToList());

        public Task<IReadOnlyList<Deal>> GetByAssignedUserIdAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Deal>)_deals.Where(d => d.AssignedUserId == userId).ToList());

        public Task<Deal?> GetWithHistoryAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_deals.FirstOrDefault(d => d.Id == id));

        public Task AddAsync(Deal entity, CancellationToken ct = default)
        {
            _deals.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Deal entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Deal entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task AddStageHistoryAsync(DealStageHistory history, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryRefusalReasonRepository : IRefusalReasonRepository
    {
        private readonly List<RefusalReason> _reasons = [];

        public Task<IReadOnlyList<RefusalReason>> FindAsync(ISpecification<RefusalReason> specification, CancellationToken ct = default)
        {
            return Task.FromResult((IReadOnlyList<RefusalReason>)_reasons.ToList());
        }

        public Task<RefusalReason?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_reasons.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<RefusalReason>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<RefusalReason>)_reasons.ToList());

        public Task AddAsync(RefusalReason entity, CancellationToken ct = default)
        {
            _reasons.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(RefusalReason entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(RefusalReason entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task<IReadOnlyList<RefusalReason>> GetActiveAsync(CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<RefusalReason>)_reasons.Where(x => x.IsActive).ToList());

        public Task<RefusalReason?> GetByNameAsync(string name, CancellationToken ct = default) =>
            Task.FromResult(_reasons.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));
    }
}
