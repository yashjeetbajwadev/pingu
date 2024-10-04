using pingu.Providers.Ngrok.Models;

namespace pingu.Providers.Ngrok;

public interface INgrokLifetimeHook
{
    Task OnCreatedAsync(TunnelResponse tunnel, CancellationToken cancellationToken);
    Task OnDestroyedAsync(TunnelResponse tunnel, CancellationToken cancellationToken);
}