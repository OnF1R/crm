using Crm.Shared.Domain;

namespace Crm.Analytics.Domain.Entities;

public class DashboardWidget : Entity
{
    public Guid UserId { get; set; }
    public string WidgetType { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Config { get; set; } = "{}";
    public int Position { get; set; }
    public DateTime CreatedAt { get; set; }

    private DashboardWidget() { }

    public static DashboardWidget Create(Guid userId, string widgetType, string title, string config, int position)
    {
        return new DashboardWidget
        {
            UserId = userId,
            WidgetType = widgetType,
            Title = title,
            Config = config,
            Position = position,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum ReportType
{
    Сделки = 1,
    Клиенты = 2,
    Задачи = 3
}

public class Report : AggregateRoot
{
    public ReportType Type { get; set; }
    public string Parameters { get; set; } = "{}";
    public DateTime GeneratedAt { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string Data { get; set; } = "{}";

    private Report() { }

    public static Report Create(ReportType type, string parameters, Guid generatedByUserId, string data)
    {
        return new Report
        {
            Type = type,
            Parameters = parameters,
            GeneratedByUserId = generatedByUserId,
            GeneratedAt = DateTime.UtcNow,
            Data = data
        };
    }
}
