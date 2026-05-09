using Crm.Analytics.Domain.Entities;

namespace Crm.Analytics.Application.DTOs;

public record DashboardDto(
    Guid UserId,
    IReadOnlyList<WidgetDto> Widgets);

public record WidgetDto(
    Guid Id,
    string WidgetType,
    string Title,
    string Config,
    int Position);

public record ReportRequestDto(
    ReportType Type,
    string Parameters,
    string? Data = null);

public record ReportResponseDto(
    Guid Id,
    string Type,
    string Parameters,
    DateTime GeneratedAt,
    Guid GeneratedByUserId,
    string Data);
