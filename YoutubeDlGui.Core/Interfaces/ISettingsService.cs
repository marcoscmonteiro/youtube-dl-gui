using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Core.Interfaces;

public interface ISettingsService
{
    AppSettings Settings { get; }
    Task LoadAsync();
    Task SaveAsync();
    Task<List<DownloadItem>> LoadHistoryAsync();
    Task SaveHistoryAsync(IEnumerable<DownloadItem> items);
}
