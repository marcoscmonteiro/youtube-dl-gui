using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Core.Interfaces;

public interface IDownloadEngineService
{
    string ResolveEngineExecutablePath(string customNameOrPath);
    string? ResolveQuickJsExecutablePath();
    bool IsEngineInstalled(string customNameOrPath = "");
    Task<bool> DownloadAsync(
        DownloadItem item, 
        IProgress<DownloadProgressReport> progress, 
        CancellationToken cancellationToken = default);
    Task<string> GetHelpAsync(string engineExecutable, CancellationToken cancellationToken = default);
    Task<string> UpdateEngineAsync(string engineExecutable, IProgress<string>? outputProgress = null, CancellationToken cancellationToken = default);
    Task<bool> DownloadLatestFromGitHubAsync(string? targetDirectory = null, IProgress<string>? outputProgress = null, CancellationToken cancellationToken = default);
    Task<bool> DownloadQuickJsFromGitHubAsync(string? targetDirectory = null, IProgress<string>? outputProgress = null, CancellationToken cancellationToken = default);
}
