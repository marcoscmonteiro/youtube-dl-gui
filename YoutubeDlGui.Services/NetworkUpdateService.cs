using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using YoutubeDlGui.Core.Interfaces;

namespace YoutubeDlGui.Services;

public class NetworkUpdateService : INetworkUpdateService
{
    private const string DefaultNetworkShare = @"\\server.cm.dev.br\Compartilhar\Apps\YtDlpGui";

    public bool IsUpdateAvailable { get; private set; }
    public string AvailableVersion { get; private set; } = string.Empty;
    public string NetworkRepositoryPath { get; private set; } = DefaultNetworkShare;

    public NetworkUpdateService()
    {
        ResolveNetworkRepositoryPath();
    }

    private void ResolveNetworkRepositoryPath()
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string localVersionFile = Path.Combine(baseDir, "version.json");
            if (File.Exists(localVersionFile))
            {
                string json = File.ReadAllText(localVersionFile);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("networkShare", out var shareElem))
                {
                    string? share = shareElem.GetString();
                    if (!string.IsNullOrWhiteSpace(share))
                    {
                        NetworkRepositoryPath = share.Trim();
                    }
                }
            }
        }
        catch
        {
            // Fallback para DefaultNetworkShare
        }
    }

    public async Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NetworkRepositoryPath))
                {
                    return false;
                }

                if (!Directory.Exists(NetworkRepositoryPath))
                {
                    return false;
                }

                string remoteVersionFile = Path.Combine(NetworkRepositoryPath, "version.json");
                if (!File.Exists(remoteVersionFile))
                {
                    return false;
                }

                string json = File.ReadAllText(remoteVersionFile);
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("version", out var verElem))
                {
                    return false;
                }

                string? remoteVerStr = verElem.GetString();
                if (string.IsNullOrWhiteSpace(remoteVerStr))
                {
                    return false;
                }

                Version currentVer = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(2, 0, 0);
                if (Version.TryParse(remoteVerStr, out var remoteVer))
                {
                    if (remoteVer > currentVer)
                    {
                        IsUpdateAvailable = true;
                        AvailableVersion = $"v{remoteVer.Major}.{remoteVer.Minor}.{Math.Max(0, remoteVer.Build)}";
                        return true;
                    }
                }
            }
            catch
            {
                // Falha de rede ou timeout tratado silenciosamente
            }

            return false;
        }, cancellationToken);
    }

    public async Task<bool> ApplyUpdateAndRestartAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
                string exeName = "YoutubeDlGui.App.exe";
                string exePath = Path.Combine(baseDir, exeName);
                int currentPid = Environment.ProcessId;

                string updaterScriptPath = Path.Combine(Path.GetTempPath(), $"YtDlpGui_Update_{Guid.NewGuid():N}.cmd");

                string scriptContent = $@"@echo off
setlocal
timeout /t 1 /nobreak >nul
taskkill /F /PID {currentPid} >nul 2>&1
timeout /t 1 /nobreak >nul

robocopy ""{NetworkRepositoryPath}"" ""{baseDir}"" YoutubeDlGui.App.exe yt-dlp.exe qjs.exe version.json /XO /FFT /R:2 /W:2 /NP >nul 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-ChildItem '{baseDir}' -Recurse -File | Unblock-File -ErrorAction SilentlyContinue"" >nul 2>&1

start """" ""{exePath}""
del ""%~f0"" >nul 2>&1
";

                File.WriteAllText(updaterScriptPath, scriptContent);

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{updaterScriptPath}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                Process.Start(psi);
                Environment.Exit(0);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao aplicar atualização: {ex.Message}");
                return false;
            }
        });
    }
}
