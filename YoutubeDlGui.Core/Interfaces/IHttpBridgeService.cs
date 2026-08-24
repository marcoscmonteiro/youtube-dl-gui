using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Core.Interfaces;

public interface IHttpBridgeService : IDisposable
{
    bool IsRunning { get; }
    int Port { get; }
    Task StartAsync(int port = 48190);
    Task StopAsync();
    event EventHandler<ExternalDownloadRequest>? DownloadRequested;
}
