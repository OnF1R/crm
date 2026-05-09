namespace Crm.Analytics.Application.Options;

public sealed class AnalyticsServiceOptions
{
    public string ClientsServiceUrl { get; set; } = "http://clients:8080";
    public string DealsServiceUrl { get; set; } = "http://deals:8080";
    public string TasksServiceUrl { get; set; } = "http://tasks:8080";
    public int RequestPageSize { get; set; } = 200;
}

