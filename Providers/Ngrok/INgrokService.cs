using pingu.Providers.Ngrok.Models;

namespace pingu.Providers.Ngrok;

public interface INgrokService
{
    IReadOnlyCollection<TunnelResponse> ActiveTunnels { get; }

    Task WaitUntilReadyAsync(CancellationToken cancellationToken = default);

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<bool> TryInitializeAsync(CancellationToken cancellationToken = default);

    Task<TunnelResponse> StartAsync(
        Uri host,
        CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}