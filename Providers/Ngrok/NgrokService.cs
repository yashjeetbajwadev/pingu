using Microsoft.Extensions.Options;
using pingu.Providers.Ngrok.Models;

namespace pingu.Providers.Ngrok;

public class NgrokService(
    INgrokDownloader downloader,
    INgrokProcess process,
    IOptionsMonitor<NgrokOptions> options,
    IEnumerable<INgrokLifetimeHook> hooks,
    INgrokApiClient ngrok,
    ILogger<NgrokService> logger)
    : INgrokService
{
    private bool _isInitialized;

    private readonly HashSet<TunnelResponse> _activeTunnels = [];

    public IReadOnlyCollection<TunnelResponse> ActiveTunnels => _activeTunnels;

    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
    {
        while (!ActiveTunnels.Any() && !cancellationToken.IsCancellationRequested)
            await Task.Delay(25, cancellationToken);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        await downloader.DownloadExecutableAsync(cancellationToken);
        await process.StartAsync();
    }

    public async Task<bool> TryInitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await InitializeAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the ngrok tunnel.");
            return false;
        }
    }

    public async Task<TunnelResponse> StartAsync(
        Uri host,
        CancellationToken cancellationToken)
    {
        var tunnel = await GetOrCreateTunnelAsync(host, options.CurrentValue.Domain, cancellationToken);
        _activeTunnels.Clear();
        _activeTunnels.Add(tunnel);

        await Task.WhenAll(hooks
            .ToArray()
            .Select(async hook =>
            {
                try
                {
                    await hook.OnCreatedAsync(tunnel, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ngrok cook OnCreatedAsync failed.");
                }
            }));

        return tunnel;
    }

    private async Task<TunnelResponse> GetOrCreateTunnelAsync(Uri host, string? domain, CancellationToken cancellationToken)
    {
        var existingTunnels = await ngrok.GetTunnelsAsync(cancellationToken);
        var existingTunnel = existingTunnels.FirstOrDefault(x => x.Name == AppDomain.CurrentDomain.FriendlyName);
        if (existingTunnel != null)
            return existingTunnel;

        return await ngrok.CreateTunnelAsync(
            AppDomain.CurrentDomain.FriendlyName,
            host,
            domain,
            cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var hooks1 = hooks.ToArray();
        var activeTunnels = _activeTunnels.ToArray();

        _activeTunnels.Clear();
        await process.StopAsync();

        await Task.WhenAll(activeTunnels
            .Select(tunnel => Task.WhenAll(hooks1
                .Select(async hook =>
                {
                    try
                    {
                        await hook.OnDestroyedAsync(tunnel, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Ngrok cook OnDestroyedAsync failed.");
                    }
                }))));
    }
}