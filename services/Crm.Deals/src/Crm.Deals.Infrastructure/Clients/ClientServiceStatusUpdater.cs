using Crm.Deals.Application.Infrastructure.Ports;
using Crm.Deals.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace Crm.Deals.Infrastructure.Clients;

public sealed class ClientServiceStatusUpdater : IClientStatusUpdater
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ClientServiceStatusUpdater> _logger;
    private readonly string _clientsServiceUrl;
    private readonly int _activeClientStatus;
    private readonly int _closedClientStatus;

    public ClientServiceStatusUpdater(
        IHttpClientFactory httpClientFactory,
        IOptions<ClientsServiceOptions> options,
        ILogger<ClientServiceStatusUpdater> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _clientsServiceUrl = options.Value.ClientsServiceUrl.TrimEnd('/');
        _activeClientStatus = options.Value.ActiveClientStatus;
        _closedClientStatus = options.Value.ClosedClientStatus;
    }

    public Task SetClosedAsync(Guid clientId, CancellationToken ct = default) =>
        SetStatusAsync(clientId, _closedClientStatus, "closing a won deal", ct);

    public Task SetActiveAsync(Guid clientId, CancellationToken ct = default) =>
        SetStatusAsync(clientId, _activeClientStatus, "reopening or losing a deal", ct);

    private async Task SetStatusAsync(Guid clientId, int status, string reason, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("deals-clients");
        var response = await client.PutAsJsonAsync(
            $"{_clientsServiceUrl}/clients/{clientId}/status",
            new { Status = status },
            ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning(
                "Client {ClientId} was not found while {Reason}; deal stage update will continue without client status sync.",
                clientId,
                reason);
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Unable to update client '{clientId}' status in client service. Status code: {response.StatusCode}.");
        }
    }
}
