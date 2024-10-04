namespace pingu.Providers.Ngrok;

public interface INgrokDownloader
{
    Task DownloadExecutableAsync(CancellationToken cancellationToken);
}