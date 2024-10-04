namespace pingu.Providers.Ngrok;

public interface INgrokProcess
{
    Task StartAsync();
    Task StopAsync();
}