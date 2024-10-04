﻿using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace pingu.Providers.Ngrok;

public class NgrokProcess(
    IOptionsMonitor<NgrokOptions> options,
    ILogger<NgrokProcess> logger,
    INgrokApiClient ngrokApiClient)
    : INgrokProcess
{
    public async Task StartAsync()
    {
        await KillExistingProcessesAsync();

        logger.LogInformation("Starting Ngrok process...");

        var processInformation = GetProcessStartInfo();
        using var process =
            Process.Start(processInformation) ??
            throw new InvalidOperationException("Could not start process.");

        try
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
            while (!await ngrokApiClient.IsNgrokReady(cancellationToken))
            {
                await Task.Delay(100, cancellationToken);
            }
        }
        catch (TaskCanceledException ex)
        {
            throw new InvalidOperationException("Ngrok process did not start in time. This might be due to an invalid auth token or other issues.", ex);
        }
    }

    private ProcessWindowStyle GetProcessWindowStyle()
    {
        return options.CurrentValue.ShowNgrokWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
    }

    private async Task KillExistingProcessesAsync()
    {
        var existingProcesses = Process
            .GetProcessesByName(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Ngrok" : "ngrok")
            .ToArray();
        if (!existingProcesses.Any())
            return;

        try
        {
            logger.LogDebug("Killing existing ngrok processes.");

            foreach (var existingProcess in existingProcesses)
            {
                existingProcess.Kill();
                await existingProcess.WaitForExitAsync();
            }
        }
        finally
        {
            foreach (var existingProcess in existingProcesses)
            {
                existingProcess.Dispose();
            }
        }
    }

    private ProcessStartInfo GetProcessStartInfo()
    {
        var authTokenArg = !string.IsNullOrEmpty(options.CurrentValue.AuthToken)
            ? $"--authtoken {options.CurrentValue.AuthToken}"
            : throw new InvalidOperationException("An auth token is required for Ngrok to work. Sign up at https://ngrok.com to get your token and a domain.");

        var arguments = $"start --none {authTokenArg}".Trim();

        return new ProcessStartInfo(
            NgrokDownloader.GetExecutableFileName(),
            arguments)
        {
            CreateNoWindow = true,
            WindowStyle = GetProcessWindowStyle(),
            UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            WorkingDirectory = Environment.CurrentDirectory,
            RedirectStandardError = false,
            RedirectStandardOutput = false,
            RedirectStandardInput = false
        };
    }



    public async Task StopAsync()
    {
        await KillExistingProcessesAsync();
    }
}