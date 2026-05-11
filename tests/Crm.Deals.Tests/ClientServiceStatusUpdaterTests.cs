using Crm.Deals.Application.Options;
using Crm.Deals.Infrastructure.Clients;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Crm.Deals.Tests;

public sealed class ClientServiceStatusUpdaterTests
{
    [Fact]
    public async Task SetClosedAsync_ShouldNotThrowWhenClientIsMissing()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.NotFound);
        var updater = new ClientServiceStatusUpdater(
            new StubHttpClientFactory(handler),
            Options.Create(new ClientsServiceOptions
            {
                ClientsServiceUrl = "http://clients:8080",
                ActiveClientStatus = 2,
                ClosedClientStatus = 4
            }),
            NullLogger<ClientServiceStatusUpdater>.Instance);

        await updater.SetClosedAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task SetClosedAsync_ShouldThrowWhenClientServiceFails()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.InternalServerError);
        var updater = new ClientServiceStatusUpdater(
            new StubHttpClientFactory(handler),
            Options.Create(new ClientsServiceOptions
            {
                ClientsServiceUrl = "http://clients:8080",
                ActiveClientStatus = 2,
                ClosedClientStatus = 4
            }),
            NullLogger<ClientServiceStatusUpdater>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => updater.SetClosedAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task SetActiveAsync_ShouldSendConfiguredActiveStatus()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK);
        var updater = new ClientServiceStatusUpdater(
            new StubHttpClientFactory(handler),
            Options.Create(new ClientsServiceOptions
            {
                ClientsServiceUrl = "http://clients:8080",
                ActiveClientStatus = 2,
                ClosedClientStatus = 4
            }),
            NullLogger<ClientServiceStatusUpdater>.Instance);

        await updater.SetActiveAsync(Guid.NewGuid());

        using var document = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal(2, document.RootElement.GetProperty("status").GetInt32());
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public string? LastBody { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastBody = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
