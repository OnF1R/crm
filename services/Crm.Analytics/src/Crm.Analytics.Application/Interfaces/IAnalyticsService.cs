using Crm.Analytics.Application.DTOs;
using Crm.Analytics.Domain.Entities;

namespace Crm.Analytics.Application.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardDto> GetDashboardAsync(Guid userId, CancellationToken ct = default);
    Task<ReportResponseDto> GenerateReportAsync(ReportType type, string parameters, Guid generatedByUserId, string? data = null, CancellationToken ct = default);
    Task<IReadOnlyList<ReportResponseDto>> GetRecentReportsAsync(int take = 20, CancellationToken ct = default);
}
