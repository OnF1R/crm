using Crm.Analytics.Application.DTOs;
using Crm.Analytics.Domain.Entities;

namespace Crm.Analytics.Application.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardDto> GetDashboardAsync(Guid userId, CancellationToken ct = default);
    Task<ReportResponseDto> GenerateReportAsync(ReportType type, string parameters, Guid generatedByUserId, CancellationToken ct = default);
}
