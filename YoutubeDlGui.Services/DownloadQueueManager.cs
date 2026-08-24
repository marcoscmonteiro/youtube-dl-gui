using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using YoutubeDlGui.Core.Enums;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Services;

public class DownloadQueueManager : IDownloadQueueManager, IDisposable
{
    private readonly IDownloadEngineService _engineService;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();
    private SemaphoreSlim _semaphore;
    private int _maxConcurrent = 3;
    private readonly object _lockObj = new();
    private bool _isDisposed;

    public ObservableCollection<DownloadItem> Items { get; } = new();

    public int MaxConcurrentDownloads
    {
        get => _maxConcurrent;
        set
        {
            if (value < 1) value = 1;
            if (_maxConcurrent != value)
            {
                lock (_lockObj)
                {
                    _maxConcurrent = value;
                    _semaphore.Dispose();
                    _semaphore = new SemaphoreSlim(_maxConcurrent, _maxConcurrent);
                }
            }
        }
    }

    public int ActiveDownloadsCount => Items.Count(i => i.Status == DownloadStatus.Downloading || i.Status == DownloadStatus.Processing);
    public int QueuedDownloadsCount => Items.Count(i => i.Status == DownloadStatus.Queued);

    public event EventHandler<DownloadItem>? ItemStatusChanged;

    public DownloadQueueManager(IDownloadEngineService engineService)
    {
        _engineService = engineService;
        _semaphore = new SemaphoreSlim(_maxConcurrent, _maxConcurrent);
    }

    public void Enqueue(DownloadItem item)
    {
        item.Status = DownloadStatus.Queued;
        item.StatusMessage = "Queued";
        item.ProgressPercentage = 0;

        if (!Items.Contains(item))
        {
            Items.Add(item);
        }

        ItemStatusChanged?.Invoke(this, item);
        _ = ProcessQueueItemAsync(item);
    }

    public void Cancel(DownloadItem item)
    {
        if (_cancellationTokens.TryGetValue(item.Id, out var cts))
        {
            cts.Cancel();
        }
        else if (item.Status == DownloadStatus.Queued)
        {
            item.Status = DownloadStatus.Cancelled;
            item.StatusMessage = "Cancelled";
            ItemStatusChanged?.Invoke(this, item);
        }
    }

    public void Retry(DownloadItem item)
    {
        if (item.Status == DownloadStatus.Downloading || item.Status == DownloadStatus.Processing)
            return;

        Enqueue(item);
    }

    public void Remove(DownloadItem item)
    {
        Cancel(item);
        Items.Remove(item);
    }

    public void ClearCompleted()
    {
        var completedList = Items
            .Where(i => i.Status == DownloadStatus.Completed || i.Status == DownloadStatus.Cancelled || i.Status == DownloadStatus.Failed)
            .ToList();

        foreach (var item in completedList)
        {
            Items.Remove(item);
        }
    }

    public void CancelAll()
    {
        foreach (var item in Items.Where(i => i.Status == DownloadStatus.Downloading || i.Status == DownloadStatus.Queued))
        {
            Cancel(item);
        }
    }

    private async Task ProcessQueueItemAsync(DownloadItem item)
    {
        var cts = new CancellationTokenSource();
        _cancellationTokens[item.Id] = cts;

        try
        {
            await _semaphore.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            item.Status = DownloadStatus.Cancelled;
            item.StatusMessage = "Cancelled before starting";
            _cancellationTokens.TryRemove(item.Id, out _);
            ItemStatusChanged?.Invoke(this, item);
            return;
        }

        try
        {
            if (cts.IsCancellationRequested)
            {
                item.Status = DownloadStatus.Cancelled;
                item.StatusMessage = "Cancelled";
                ItemStatusChanged?.Invoke(this, item);
                return;
            }

            item.Status = DownloadStatus.Downloading;
            item.StatusMessage = "Downloading...";
            ItemStatusChanged?.Invoke(this, item);

            var progress = new Progress<DownloadProgressReport>(report =>
            {
                if (report.Percentage > 0)
                {
                    item.ProgressPercentage = report.Percentage;
                }
                if (!string.IsNullOrEmpty(report.Speed))
                {
                    item.DownloadSpeed = report.Speed;
                }
                if (!string.IsNullOrEmpty(report.Eta))
                {
                    item.Eta = report.Eta;
                }
                if (!string.IsNullOrEmpty(report.TotalSize))
                {
                    item.TotalSize = report.TotalSize;
                }
                if (!string.IsNullOrEmpty(report.ExtractedFileName))
                {
                    item.FileName = report.ExtractedFileName;
                }
                if (!string.IsNullOrEmpty(report.StatusText))
                {
                    item.StatusMessage = report.StatusText;
                }

                ItemStatusChanged?.Invoke(this, item);
            });

            bool success = await _engineService.DownloadAsync(item, progress, cts.Token);

            if (cts.IsCancellationRequested)
            {
                item.Status = DownloadStatus.Cancelled;
                item.StatusMessage = "Cancelled";
            }
            else if (success)
            {
                item.Status = DownloadStatus.Completed;
                item.StatusMessage = "Completed";
                item.ProgressPercentage = 100;
                item.CompletedAt = DateTime.Now;
            }
            else
            {
                item.Status = DownloadStatus.Failed;
                item.StatusMessage = "Error downloading video";
            }
        }
        catch (OperationCanceledException)
        {
            item.Status = DownloadStatus.Cancelled;
            item.StatusMessage = "Cancelled";
        }
        catch (Exception ex)
        {
            item.Status = DownloadStatus.Failed;
            item.StatusMessage = $"Failed: {ex.Message}";
        }
        finally
        {
            _semaphore.Release();
            _cancellationTokens.TryRemove(item.Id, out _);
            ItemStatusChanged?.Invoke(this, item);
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            CancelAll();
            _semaphore.Dispose();
        }
    }
}
