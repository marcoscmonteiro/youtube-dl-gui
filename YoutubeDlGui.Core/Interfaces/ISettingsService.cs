using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Core.Interfaces;

public interface ISettingsService
{
    AppSettings Settings { get; }
    string StorageFolder { get; }
    bool IsCloudSynced { get; }
    Task LoadAsync();
    Task SaveAsync();
    Task<List<DownloadItem>> LoadHistoryAsync();
    Task SaveHistoryAsync(IEnumerable<DownloadItem> items);
}
