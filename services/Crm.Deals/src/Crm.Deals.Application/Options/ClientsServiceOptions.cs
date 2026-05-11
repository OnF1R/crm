namespace Crm.Deals.Application.Options;

public sealed class ClientsServiceOptions
{
    public string ClientsServiceUrl { get; set; } = "http://clients:8080";
    public int ActiveClientStatus { get; set; } = 2;
    public int ClosedClientStatus { get; set; } = 4;
}
