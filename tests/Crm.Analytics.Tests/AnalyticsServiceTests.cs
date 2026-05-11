using Crm.Analytics.Application;
using Crm.Analytics.Application.Options;
using Crm.Analytics.Application.Services;
using Crm.Analytics.Domain.Entities;
using Crm.Analytics.Domain.Interfaces;
using Crm.Shared.Domain;
using Crm.Shared.DTOs;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Crm.Analytics.Tests;

public sealed class AnalyticsServiceTests
{
    [Fact]
    public async Task GenerateReportAsync_ShouldCalculateActiveWonAndLostDealAggregatesFromNormalizedStages()
    {
        var service = CreateService(
            deals: new[]
            {
                new DealSnapshotDto(100m, "RUB", "1", DateTime.UtcNow.AddDays(-10), null),
                new DealSnapshotDto(50m, "RUB", "2", DateTime.UtcNow.AddDays(-9), null),
                new DealSnapshotDto(1000m, "RUB", "ЗакрытиеУспех", DateTime.UtcNow.AddDays(-8), DateTime.UtcNow.AddDays(-1)),
                new DealSnapshotDto(200m, "USD", "Won", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(-1)),
                new DealSnapshotDto(5m, "RUB", "ЗакрытиеПровал", DateTime.UtcNow.AddDays(-6), DateTime.UtcNow.AddDays(-1)),
                new DealSnapshotDto(3m, "RUB", "ClosedLost", DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1)),
            },
            tasks: Array.Empty<TaskSnapshotDto>(),
            clients: Array.Empty<ClientSnapshotDto>());

        var report = await service.GenerateReportAsync((ReportType)1, "{}", Guid.NewGuid(), null, default);
        using var doc = JsonDocument.Parse(report.Data);
        var data = doc.RootElement;

        Assert.Equal(2, data.GetProperty("activeDeals").GetInt32());
        Assert.Equal(2, data.GetProperty("wonDeals").GetInt32());
        Assert.Equal(2, data.GetProperty("lostDeals").GetInt32());
        Assert.Equal(150m, data.GetProperty("totalAmount").GetDecimal());
        Assert.Equal(6, data.GetProperty("totalDeals").GetInt32());
    }

    [Fact]
    public async Task GenerateReportAsync_ShouldCalculateTaskSummaryWithCompletedAndPendingNormalization()
    {
        var pastDate = DateTime.UtcNow.AddMinutes(-10);
        var futureDate = DateTime.UtcNow.AddMinutes(10);

        var service = CreateService(
            deals: Array.Empty<DealSnapshotDto>(),
            tasks: new[]
            {
                new TaskSnapshotDto("Завершена", "1", DateTime.UtcNow.AddDays(-2), pastDate),
                new TaskSnapshotDto("4", "2", DateTime.UtcNow.AddDays(-2), pastDate),
                new TaskSnapshotDto("Completed", "3", DateTime.UtcNow.AddDays(-2), null),
                new TaskSnapshotDto("Отменена", "4", DateTime.UtcNow.AddDays(-2), pastDate),
                new TaskSnapshotDto("5", "4", DateTime.UtcNow.AddDays(-2), pastDate),
                new TaskSnapshotDto("New", "1", DateTime.UtcNow.AddDays(-1), pastDate),
                new TaskSnapshotDto("In progress", "2", DateTime.UtcNow.AddDays(-1), futureDate),
            },
            clients: Array.Empty<ClientSnapshotDto>());

        var report = await service.GenerateReportAsync((ReportType)3, "{}", Guid.NewGuid(), null, default);
        using var doc = JsonDocument.Parse(report.Data);
        var data = doc.RootElement;

        Assert.Equal(7, data.GetProperty("totalTasks").GetInt32());
        Assert.Equal(3, data.GetProperty("completedTasks").GetInt32());
        Assert.Equal(2, data.GetProperty("pendingTasks").GetInt32());
        Assert.Equal(1, data.GetProperty("overdueTasks").GetInt32());
    }

    private static AnalyticsService CreateService(
        IReadOnlyList<DealSnapshotDto> deals,
        IReadOnlyList<TaskSnapshotDto> tasks,
        IReadOnlyList<ClientSnapshotDto> clients)
    {
        var handler = new FakeHttpMessageHandler(deals, tasks, clients);

        var service = new AnalyticsService(
            new FakeHttpClientFactory(handler),
            new InMemoryReportRepository(),
            new FakeUnitOfWork(),
            Options.Create(new AnalyticsServiceOptions
            {
                ClientsServiceUrl = "http://localhost",
                DealsServiceUrl = "http://localhost",
                TasksServiceUrl = "http://localhost",
                RequestPageSize = 200
            }));

        return service;
    }

    private sealed class FakeHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(handler, disposeHandler: false)
            {
                BaseAddress = new Uri("http://localhost/")
            };
        }
    }

    private sealed class FakeHttpMessageHandler(
        IReadOnlyList<DealSnapshotDto> deals,
        IReadOnlyList<TaskSnapshotDto> tasks,
        IReadOnlyList<ClientSnapshotDto> clients) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken ct)
        {
            var endpoint = request.RequestUri!.AbsolutePath.Trim('/').ToLowerInvariant();
            var response = endpoint switch
            {
                "deals" => CreatePagedResponse(deals),
                "tasks" => CreatePagedResponse(tasks),
                "clients" => CreatePagedResponse(clients),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            };

            return Task.FromResult(response);
        }

        private static HttpResponseMessage CreatePagedResponse<T>(IReadOnlyList<T> items)
        {
            var data = ApiResponse<PagedResponse<T>>.Ok(
                new PagedResponse<T>(items, items.Count, 1, 200));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(data, _jsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };
        }

        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private sealed class InMemoryReportRepository : IReportRepository
    {
        public Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult<Report?>(null);

        public Task<IReadOnlyList<Report>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Report>)Array.Empty<Report>());

        public Task<IReadOnlyList<Report>> FindAsync(
            ISpecification<Report> specification,
            CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Report>)Array.Empty<Report>());

        public Task AddAsync(Report entity, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Report entity, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task DeleteAsync(Report entity, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => Task.FromResult(0);

        public void Dispose()
        {
        }
    }

    private sealed record DealSnapshotDto(
        decimal Amount,
        string Currency,
        string Stage,
        DateTime CreatedAt,
        DateTime? ClosedAt);

    private sealed record ClientSnapshotDto(
        string Status,
        DateTime CreatedAt);

    private sealed record TaskSnapshotDto(
        string Status,
        string Priority,
        DateTime CreatedAt,
        DateTime? DueDate);
}
