using Crm.Analytics.Application.DTOs;
using Crm.Analytics.Application.Interfaces;
using Crm.Analytics.Application.Options;
using Crm.Analytics.Domain.Entities;
using Crm.Analytics.Domain.Interfaces;
using Crm.Shared.Domain;
using Crm.Shared.DTOs;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Crm.Analytics.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private const int DealsReportType = 1;
    private const int ClientsReportType = 2;
    private const int TasksReportType = 3;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _clientsServiceUrl;
    private readonly string _dealsServiceUrl;
    private readonly string _tasksServiceUrl;
    private readonly int _requestPageSize;

    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public AnalyticsService(
        IHttpClientFactory httpClientFactory,
        IReportRepository reportRepository,
        IUnitOfWork unitOfWork,
        IOptions<AnalyticsServiceOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
        _clientsServiceUrl = NormalizeBaseUrl(options.Value.ClientsServiceUrl, "http://clients:8080");
        _dealsServiceUrl = NormalizeBaseUrl(options.Value.DealsServiceUrl, "http://deals:8080");
        _tasksServiceUrl = NormalizeBaseUrl(options.Value.TasksServiceUrl, "http://tasks:8080");
        _requestPageSize = options.Value.RequestPageSize <= 0 ? 200 : options.Value.RequestPageSize;
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid userId, CancellationToken ct = default)
    {
        var snapshot = await LoadSnapshotAsync(null, ct);

        var widgets = new List<WidgetDto>
        {
            new(
                Guid.NewGuid(),
                "deals_summary",
                "Deals summary",
                BuildDealsSummary(snapshot.Deals),
                0),
            new(
                Guid.NewGuid(),
                "tasks_summary",
                "Tasks summary",
                BuildTasksSummary(snapshot.Tasks),
                1),
            new(
                Guid.NewGuid(),
                "clients_summary",
                "Clients summary",
                BuildClientsSummary(snapshot.Clients),
                2)
        };

        return new DashboardDto(userId, widgets);
    }

    public async Task<IReadOnlyList<ReportResponseDto>> GetRecentReportsAsync(int take = 20, CancellationToken ct = default)
    {
        var normalizedTake = Math.Clamp(take, 1, 100);
        var reports = await _reportRepository.GetAllAsync(ct);

        return reports
            .OrderByDescending(x => x.GeneratedAt)
            .Take(normalizedTake)
            .Select(r => new ReportResponseDto(
                r.Id,
                ((int)r.Type).ToString(),
                r.Parameters,
                r.GeneratedAt,
                r.GeneratedByUserId,
                r.Data))
            .ToList();
    }

    public async Task<ReportResponseDto> GenerateReportAsync(
        ReportType type,
        string parameters,
        Guid generatedByUserId,
        string? data = null,
        CancellationToken ct = default)
    {
        parameters = string.IsNullOrWhiteSpace(parameters) ? "{}" : parameters;
        var period = ParsePeriodFromParameters(parameters);

        var reportData = !string.IsNullOrWhiteSpace(data)
            ? data
            : await BuildReportDataAsync(type, period, ct);

        var report = Report.Create(type, parameters, generatedByUserId, reportData);
        await _reportRepository.AddAsync(report, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new ReportResponseDto(
            report.Id,
            ((int)report.Type).ToString(),
            report.Parameters,
            report.GeneratedAt,
            report.GeneratedByUserId,
            report.Data);
    }

    private async Task<PeriodSnapshot> LoadSnapshotAsync(ReportPeriod? period, CancellationToken ct)
    {
        var from = period?.From;
        var to = period?.To;

        var dealsTask = GetDealsAsync(from, to, ct);
        var clientsTask = GetClientsAsync(from, to, ct);
        var tasksTask = GetTasksAsync(from, to, ct);

        await Task.WhenAll(dealsTask, clientsTask, tasksTask);

        return new PeriodSnapshot(
            await dealsTask,
            await clientsTask,
            await tasksTask);
    }

    private async Task<string> BuildReportDataAsync(ReportType type, ReportPeriod period, CancellationToken ct)
    {
        var snapshot = await LoadSnapshotAsync(period, ct);

        return ((int)type) switch
        {
            DealsReportType => BuildDealsReport(snapshot.Deals, period),
            ClientsReportType => BuildClientsReport(snapshot.Clients, period),
            TasksReportType => BuildTasksReport(snapshot.Tasks, period),
            _ => JsonSerializer.Serialize(new { unknownType = true }, _jsonOptions)
        };
    }

    private string BuildDealsReport(IReadOnlyList<DealSnapshot> deals, ReportPeriod period)
    {
        var byStage = BuildDistribution(deals, x => NormalizeDealStage(x.Stage));
        var byMonth = BuildTimeDistribution(deals, x => x.CreatedAt, period);
        var byCurrency = BuildDistribution(deals, x => NormalizeCurrency(x.Currency));
        var activeDeals = deals.Count(d => IsDealActive(d.Stage));
        var wonDeals = deals.Count(d => IsDealWon(d.Stage));
        var lostDeals = deals.Count(d => IsDealLost(d.Stage));

        return JsonSerializer.Serialize(new
        {
            type = "deals",
            period = new
            {
                from = period.From.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                to = period.To.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
            },
            totalDeals = deals.Count,
            activeDeals,
            wonDeals,
            lostDeals,
            totalAmount = deals
                .Where(d => IsDealActive(d.Stage) && IsRubCurrency(d.Currency))
                .Sum(d => d.Amount),
            byCurrency,
            byStage,
            byMonth
        }, _jsonOptions);
    }

    private string BuildClientsReport(IReadOnlyList<ClientSnapshot> clients, ReportPeriod period)
    {
        var byStatus = BuildDistribution(clients, x => NormalizeClientStatus(x.Status));
        var createdByMonth = BuildTimeDistribution(clients, x => x.CreatedAt, period);
        var newClients = clients.Count(c => c.CreatedAt >= period.From && c.CreatedAt <= period.To);

        return JsonSerializer.Serialize(new
        {
            type = "clients",
            period = new
            {
                from = period.From.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                to = period.To.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
            },
            totalClients = clients.Count,
            newClients,
            byStatus,
            byMonth = createdByMonth
        }, _jsonOptions);
    }

    private string BuildTasksReport(IReadOnlyList<TaskSnapshot> tasks, ReportPeriod period)
    {
        var byStatus = BuildDistribution(tasks, x => NormalizeTaskStatus(x.Status));
        var byPriority = BuildDistribution(tasks, x => NormalizeTaskPriority(x.Priority));
        var byDueMonth = BuildTimeDistribution(tasks, x => x.CreatedAt, period);
        var completed = tasks.Count(t => IsTaskCompleted(t.Status));
        var pending = tasks.Count(t => IsOpenTaskStatus(t.Status));
        var overdue = tasks.Count(t =>
            t.DueDate.HasValue && IsOpenTaskStatus(t.Status) && t.DueDate.Value < DateTime.UtcNow);

        return JsonSerializer.Serialize(new
        {
            type = "tasks",
            period = new
            {
                from = period.From.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                to = period.To.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
            },
            totalTasks = tasks.Count,
            completedTasks = completed,
            pendingTasks = pending,
            overdueTasks = overdue,
            byStatus,
            byPriority,
            byMonth = byDueMonth
        }, _jsonOptions);
    }

    private string BuildDealsSummary(IReadOnlyList<DealSnapshot> deals)
    {
        var byStage = BuildDistribution(deals, x => NormalizeDealStage(x.Stage));
        var byMonth = BuildTimeDistribution(deals, x => x.CreatedAt, BuildDefaultTrendPeriod());
        var byCurrency = BuildDistribution(deals, x => NormalizeCurrency(x.Currency));
        var activeDeals = deals.Count(d => IsDealActive(d.Stage));
        var wonDeals = deals.Count(d => IsDealWon(d.Stage));
        var lostDeals = deals.Count(d => IsDealLost(d.Stage));

        return JsonSerializer.Serialize(new
        {
            totalDeals = deals.Count,
            activeDeals,
            wonDeals,
            lostDeals,
            totalAmount = deals
                .Where(d => IsDealActive(d.Stage) && IsRubCurrency(d.Currency))
                .Sum(d => d.Amount),
            byStage,
            byCurrency,
            byMonth
        }, _jsonOptions);
    }

    private string BuildTasksSummary(IReadOnlyList<TaskSnapshot> tasks)
    {
        var byStatus = BuildDistribution(tasks, x => NormalizeTaskStatus(x.Status));
        var byPriority = BuildDistribution(tasks, x => NormalizeTaskPriority(x.Priority));
        var byMonth = BuildTimeDistribution(tasks, x => x.CreatedAt, BuildDefaultTrendPeriod());
        var overdue = tasks.Count(t =>
            t.DueDate.HasValue && IsOpenTaskStatus(t.Status) && t.DueDate.Value < DateTime.UtcNow);

        return JsonSerializer.Serialize(new
        {
            totalTasks = tasks.Count,
            completedTasks = tasks.Count(t => IsTaskCompleted(t.Status)),
            pendingTasks = tasks.Count(t => IsOpenTaskStatus(t.Status)),
            overdueTasks = overdue,
            byStatus,
            byPriority,
            byMonth
        }, _jsonOptions);
    }

    private string BuildClientsSummary(IReadOnlyList<ClientSnapshot> clients)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        return JsonSerializer.Serialize(new
        {
            totalClients = clients.Count,
            newThisMonth = clients.Count(c => c.CreatedAt >= monthStart),
            byStatus = BuildDistribution(clients, x => NormalizeClientStatus(x.Status))
        }, _jsonOptions);
    }

    private async Task<IReadOnlyList<DealSnapshot>> GetDealsAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct)
    {
        var all = await FetchAllAsync<DealSnapshot>(_dealsServiceUrl, "deals", ct);

        if (fromUtc is null || toUtc is null)
        {
            return all;
        }

        return all
            .Where(d => d.CreatedAt >= fromUtc.Value && d.CreatedAt <= toUtc.Value)
            .ToList();
    }

    private async Task<IReadOnlyList<ClientSnapshot>> GetClientsAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct)
    {
        var all = await FetchAllAsync<ClientSnapshot>(_clientsServiceUrl, "clients", ct);

        if (fromUtc is null || toUtc is null)
        {
            return all;
        }

        return all
            .Where(c => c.CreatedAt >= fromUtc.Value && c.CreatedAt <= toUtc.Value)
            .ToList();
    }

    private async Task<IReadOnlyList<TaskSnapshot>> GetTasksAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct)
    {
        var all = await FetchAllAsync<TaskSnapshot>(_tasksServiceUrl, "tasks", ct);

        if (fromUtc is null || toUtc is null)
        {
            return all;
        }

        return all
            .Where(t => t.CreatedAt >= fromUtc.Value && t.CreatedAt <= toUtc.Value)
            .ToList();
    }

    private async Task<List<T>> FetchAllAsync<T>(string serviceUrl, string endpoint, CancellationToken ct)
    {
        var all = new List<T>();
        var page = 1;

        while (true)
        {
            var pageData = await FetchPageAsync<T>(serviceUrl, endpoint, page, ct);
            if (pageData is null)
            {
                break;
            }

            all.AddRange(pageData.Items);

            if (!pageData.HasNext || pageData.Items.Count == 0)
            {
                break;
            }

            page++;
        }

        return all;
    }

    private async Task<PagedResponse<T>?> FetchPageAsync<T>(string serviceUrl, string endpoint, int page, CancellationToken ct)
    {
        var url = $"{serviceUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}?page={page}&pageSize={_requestPageSize}";
        using var client = _httpClientFactory.CreateClient("analytics-services");

        try
        {
            using var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponse<T>>>(cancellationToken: ct);
            if (apiResponse is null || !apiResponse.Success || apiResponse.Data is null)
            {
                return null;
            }

            return apiResponse.Data;
        }
        catch
        {
            return null;
        }
    }

    private static string NormalizeBaseUrl(string value, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim().TrimEnd('/');
        }

        return fallback;
    }

    private static ReportPeriod ParsePeriodFromParameters(string parameters)
    {
        var to = DateTime.UtcNow;
        var from = to.AddDays(-30);

        if (string.IsNullOrWhiteSpace(parameters))
        {
            return new ReportPeriod(from, to);
        }

        try
        {
            using var document = JsonDocument.Parse(parameters);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new ReportPeriod(from, to);
            }

            if (document.RootElement.TryGetProperty("period", out var periodElement) &&
                periodElement.ValueKind == JsonValueKind.String)
            {
                var shortcut = periodElement.GetString();
                if (!string.IsNullOrWhiteSpace(shortcut))
                {
                    var resolved = ResolvePeriodShortcut(shortcut.Trim().ToLowerInvariant(), to);
                    if (resolved is not null)
                    {
                        return resolved;
                    }
                }
            }

            if (document.RootElement.TryGetProperty("from", out var fromElement))
            {
                if (DateTime.TryParse(fromElement.GetString(), CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var parsedFrom))
                {
                    from = parsedFrom.Kind == DateTimeKind.Utc ? parsedFrom : parsedFrom.ToUniversalTime();
                }
            }

            if (document.RootElement.TryGetProperty("to", out var toElement))
            {
                if (DateTime.TryParse(toElement.GetString(), CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var parsedTo))
                {
                    to = parsedTo.Kind == DateTimeKind.Utc ? parsedTo : parsedTo.ToUniversalTime();
                }
            }

            if (from > to)
            {
                (from, to) = (to, from);
            }
        }
        catch
        {
            return new ReportPeriod(from, to);
        }

        return new ReportPeriod(from, to);
    }

    private static ReportPeriod? ResolvePeriodShortcut(string shortcut, DateTime nowUtc)
    {
        return shortcut switch
        {
            "today" => new ReportPeriod(nowUtc.Date, nowUtc.Date.AddDays(1).AddTicks(-1)),
            "yesterday" => new ReportPeriod(nowUtc.Date.AddDays(-1), nowUtc.Date.AddTicks(-1)),
            "week" => new ReportPeriod(nowUtc.Date.AddDays(-7), nowUtc),
            "7" or "7d" or "7days" => new ReportPeriod(nowUtc.AddDays(-7), nowUtc),
            "30" or "30d" or "30days" or "month" => new ReportPeriod(nowUtc.AddDays(-30), nowUtc),
            "90" or "quarter" => new ReportPeriod(nowUtc.AddDays(-90), nowUtc),
            "365" or "365d" or "year" => new ReportPeriod(nowUtc.AddDays(-365), nowUtc),
            _ => null
        };
    }

    private static Dictionary<string, int> BuildDistribution<T>(IEnumerable<T> items, Func<T, string> selector)
    {
        return items
            .GroupBy(selector)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static Dictionary<string, int> BuildMonthDistribution<T>(IEnumerable<T> items, Func<T, DateTime> dateSelector, string prefix)
    {
        return items
            .Where(item => dateSelector(item) != default)
            .GroupBy(item => dateSelector(item).ToUniversalTime().ToString("yyyy-MM", CultureInfo.InvariantCulture))
            .OrderBy(g => g.Key)
            .ToDictionary(g => $"{prefix}: {g.Key}", g => g.Count());
    }

    private static ReportPeriod BuildDefaultTrendPeriod()
    {
        var to = DateTime.UtcNow;
        return new ReportPeriod(to.AddDays(-90), to);
    }

    private static Dictionary<string, int> BuildTimeDistribution<T>(
        IEnumerable<T> items,
        Func<T, DateTime> dateSelector,
        ReportPeriod period)
    {
        var from = period.From.ToUniversalTime().Date;
        var to = period.To.ToUniversalTime().Date;
        if (from > to)
        {
            (from, to) = (to, from);
        }

        var totalDays = Math.Max(1, (to - from).TotalDays);
        var bucket = totalDays <= 45
            ? TimeBucket.Day
            : totalDays <= 180
                ? TimeBucket.Week
                : TimeBucket.Month;

        var buckets = BuildEmptyBuckets(from, to, bucket);

        foreach (var item in items)
        {
            var date = dateSelector(item);
            if (date == default)
            {
                continue;
            }

            var itemDate = date.ToUniversalTime().Date;
            if (itemDate < from || itemDate > to)
            {
                continue;
            }

            var key = FormatBucketKey(GetBucketStart(itemDate, from, bucket), bucket);
            if (buckets.ContainsKey(key))
            {
                buckets[key]++;
            }
        }

        return buckets;
    }

    private static Dictionary<string, int> BuildEmptyBuckets(DateTime from, DateTime to, TimeBucket bucket)
    {
        var result = new Dictionary<string, int>();
        var cursor = GetBucketStart(from, from, bucket);

        while (cursor <= to)
        {
            result[FormatBucketKey(cursor, bucket)] = 0;
            cursor = bucket switch
            {
                TimeBucket.Day => cursor.AddDays(1),
                TimeBucket.Week => cursor.AddDays(7),
                _ => cursor.AddMonths(1)
            };
        }

        return result;
    }

    private static DateTime GetBucketStart(DateTime date, DateTime periodFrom, TimeBucket bucket)
    {
        return bucket switch
        {
            TimeBucket.Day => date.Date,
            TimeBucket.Week => periodFrom.Date.AddDays(Math.Floor((date.Date - periodFrom.Date).TotalDays / 7) * 7),
            _ => new DateTime(date.Year, date.Month, 1)
        };
    }

    private static string FormatBucketKey(DateTime bucketStart, TimeBucket bucket)
    {
        return bucket switch
        {
            TimeBucket.Day => bucketStart.ToString("dd.MM", CultureInfo.InvariantCulture),
            TimeBucket.Week => $"{bucketStart:dd.MM}-{bucketStart.AddDays(6):dd.MM}",
            _ => bucketStart.ToString("yyyy-MM", CultureInfo.InvariantCulture)
        };
    }

    private static string NormalizeDealStage(string stage)
    {
        var normalized = stage?.Trim();

        return normalized switch
        {
            "1" => "Lead",
            "2" => "Qualification",
            "3" => "Negotiation",
            "4" => "Proposal",
            "5" or "ЗакрытиеУспех" or "ClosedWon" or "Won" or "Success" => "Won",
            "6" or "ЗакрытиеПровал" or "ClosedLost" or "Lost" or "Fail" or "Failed" => "Lost",
            _ => string.IsNullOrWhiteSpace(normalized) ? "Unknown" : normalized
        };
    }

    private static string NormalizeClientStatus(string status)
    {
        return string.IsNullOrWhiteSpace(status) ? "Unknown" : status;
    }

    private static string NormalizeTaskStatus(string status)
    {
        var normalized = status?.Trim();

        return normalized switch
        {
            "1" => "New",
            "2" => "In progress",
            "3" => "In review",
            "4" or "Завершена" or "Done" or "Completed" => "Completed",
            "5" or "Отменена" or "Canceled" or "Cancelled" => "Canceled",
            _ => string.IsNullOrWhiteSpace(normalized) ? "Unknown" : normalized
        };
    }

    private static string NormalizeTaskPriority(string priority)
    {
        return priority switch
        {
            "1" => "Low",
            "2" => "Medium",
            "3" => "High",
            "4" => "Critical",
            _ => string.IsNullOrWhiteSpace(priority) ? "Unknown" : priority
        };
    }

    private static string NormalizeCurrency(string currency)
    {
        return currency switch
        {
            "1" or "RUB" or "rub" => "RUB",
            "2" or "USD" or "usd" => "USD",
            "3" or "EUR" or "eur" => "EUR",
            _ => string.IsNullOrWhiteSpace(currency) ? "UNKNOWN" : currency
        };
    }

    private static bool IsDealActive(string stage)
    {
        return !IsDealWon(stage) && !IsDealLost(stage);
    }

    private static bool IsDealWon(string stage)
    {
        return stage?.Trim() is "5" or "ЗакрытиеУспех" or "Won" or "ClosedWon" or "Success";
    }

    private static bool IsDealLost(string stage)
    {
        return stage?.Trim() is "6" or "ЗакрытиеПровал" or "Lost" or "ClosedLost" or "Fail" or "Failed";
    }

    private static bool IsTaskCompleted(string status)
    {
        return status?.Trim() is "4" or "Завершена" or "Done" or "Completed";
    }

    private static bool IsTaskCanceled(string status)
    {
        return status?.Trim() is "5" or "Отменена" or "Canceled" or "Cancelled";
    }

    private static bool IsOpenTaskStatus(string status)
    {
        return !IsTaskCompleted(status) && !IsTaskCanceled(status);
    }

    private static bool IsRubCurrency(string currency)
    {
        return currency is "1" or "RUB" or "rub";
    }

    private sealed record ReportPeriod(DateTime From, DateTime To);

    private enum TimeBucket
    {
        Day,
        Week,
        Month
    }

    private sealed record DealSnapshot(
        decimal Amount,
        string Currency,
        string Stage,
        DateTime CreatedAt,
        DateTime? ClosedAt);

    private sealed record ClientSnapshot(
        string Status,
        DateTime CreatedAt);

    private sealed record TaskSnapshot(
        string Status,
        string Priority,
        DateTime CreatedAt,
        DateTime? DueDate);

    private sealed record PeriodSnapshot(
        IReadOnlyList<DealSnapshot> Deals,
        IReadOnlyList<ClientSnapshot> Clients,
        IReadOnlyList<TaskSnapshot> Tasks);
}


