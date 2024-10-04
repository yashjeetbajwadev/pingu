using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace pingu.Providers.Ngrok;

public class NgrokHostedService(
    IServer server,
    IHostApplicationLifetime lifetime,
    INgrokService service,
    ILogger<NgrokHostedService> logger)
    : INgrokHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await service.TryInitializeAsync(cancellationToken);

        var combinedCancellationToken = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken, lifetime.ApplicationStopping)
            .Token;

        lifetime.ApplicationStarted.Register(() =>
        {
            logger.LogDebug("Application has started - will start Ngrok.");

            var feature = server.Features.Get<IServerAddressesFeature>();
            if (feature == null)
                throw new InvalidOperationException("Ngrok requires the IServerAddressesFeature to be accessible.");

            var address = feature.Addresses
                .Select(x => new Uri(x))
                .OrderByDescending(x => x.Scheme == "http" ? 1 : 0)
                .First();
            service.StartAsync(address, combinedCancellationToken);
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Application has stopped - will stop Ngrok.");
        return service.StopAsync(cancellationToken);
    }
}