using pingu.Providers.Ngrok.Models;

namespace pingu.Providers.Ngrok;

public interface INgrokApiClient
{
    Task<TunnelResponse> CreateTunnelAsync(
        string projectName,
        Uri address,
        string? doamin,
        CancellationToken cancellationToken);

    Task<TunnelResponse[]> GetTunnelsAsync(CancellationToken cancellationToken);
    Task<bool> IsNgrokReady(CancellationToken cancellationToken);
}