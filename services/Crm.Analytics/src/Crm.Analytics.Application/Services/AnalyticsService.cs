using Crm.Analytics.Application.DTOs;
using Crm.Analytics.Application.Interfaces;
using Crm.Analytics.Domain.Entities;
using Crm.Analytics.Domain.Interfaces;
using Crm.Shared.Domain;
using System.Text.Json;

namespace Crm.Analytics.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsService(IReportRepository reportRepository, IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid userId, CancellationToken ct = default)
    {
        var widgets = new List<WidgetDto>
        {
            new(Guid.NewGuid(), "deals_summary", "Сводка по сделкам", JsonSerializer.Serialize(new { totalDeals = 0, totalAmount = 0m }), 0),
            new(Guid.NewGuid(), "tasks_summary", "Сводка по задачам", JsonSerializer.Serialize(new { totalTasks = 0, completedTasks = 0 }), 1),
            new(Guid.NewGuid(), "clients_summary", "Сводка по клиентам", JsonSerializer.Serialize(new { totalClients = 0 }), 2),
            new(Guid.NewGuid(), "recent_activities", "Последние действия", JsonSerializer.Serialize(new { activities = Array.Empty<object>() }), 3)
        };
        return new DashboardDto(userId, widgets);
    }

    public async Task<ReportResponseDto> GenerateReportAsync(ReportType type, string parameters, Guid generatedByUserId, CancellationToken ct = default)
    {
        var placeholderData = type switch
        {
            ReportType.Сделки => JsonSerializer.Serialize(new { totalDeals = 0, byStage = new { }, totalAmount = 0m }),
            ReportType.Клиенты => JsonSerializer.Serialize(new { totalClients = 0, byStatus = new { } }),
            ReportType.Задачи => JsonSerializer.Serialize(new { totalTasks = 0, byStatus = new { } }),
            _ => "{}"
        };
        var report = Report.Create(type, parameters, generatedByUserId, placeholderData);
        await _reportRepository.AddAsync(report, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return new ReportResponseDto(report.Id, report.Type.ToString(), report.Parameters, report.GeneratedAt, report.GeneratedByUserId, report.Data);
    }
}
