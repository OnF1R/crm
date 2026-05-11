namespace Crm.Deals.Application.Infrastructure.Ports;

public interface IClientStatusUpdater
{
    Task SetClosedAsync(Guid clientId, CancellationToken ct = default);
    Task SetActiveAsync(Guid clientId, CancellationToken ct = default);
}
