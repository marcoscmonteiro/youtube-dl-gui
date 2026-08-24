using System.Collections.ObjectModel;
using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Core.Interfaces;

public interface IDownloadQueueManager
{
    ObservableCollection<DownloadItem> Items { get; }
    int ActiveDownloadsCount { get; }
    int QueuedDownloadsCount { get; }
    int MaxConcurrentDownloads { get; set; }

    void Enqueue(DownloadItem item);
    void Cancel(DownloadItem item);
    void Retry(DownloadItem item);
    void Remove(DownloadItem item);
    void ClearCompleted();
    void CancelAll();

    event EventHandler<DownloadItem>? ItemStatusChanged;
}
