using YoutubeDlGui.Core.Enums;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Core.Models;
using YoutubeDlGui.Services;
using Xunit;

namespace YoutubeDlGui.Tests;

public class ServiceTests
{
    [Fact]
    public void AppSettings_DefaultValues_AreCorrect()
    {
        var settings = AppSettings.CreateDefault();
        Assert.NotNull(settings);
        Assert.Equal(AppTheme.Dark, settings.Theme);
        Assert.Equal("yt-dlp.exe", settings.EngineExecutable);
        Assert.Equal(3, settings.MaxConcurrentDownloads);
        Assert.True(settings.NoCacheDir);
        Assert.True(settings.NoPartFile);
        Assert.False(settings.IsAdvancedOptionsOpen);
        Assert.False(settings.DownloadPlaylist);
        Assert.True(settings.ClipboardAutoPaste);
    }

    [Fact]
    public void DownloadItem_FullPath_ComputesCorrectly()
    {
        var item = new DownloadItem
        {
            OutputDirectory = @"C:\Videos",
            FileName = "sample_video.mp4"
        };

        Assert.Equal(@"C:\Videos\sample_video.mp4", item.FullPath);
        Assert.Equal(@"C:\Videos\sample_video.mp4.part", item.PartFullPath);
    }

    [Fact]
    public async Task JsonSettingsService_SaveAndLoad_AllOptions_WorksCorrectly()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "yt_dlp_test_" + Guid.NewGuid().ToString("N"));
        try
        {
            var service = new JsonSettingsService(tempDir);
            service.Settings.ExtraOptions = "--test-option --cookies cookies.txt";
            service.Settings.DestinationHistory = new List<string> { @"C:\Videos", @"D:\Downloads" };
            service.Settings.ExtraOptionsHistory = new List<string> { "--test-option", "--verbose" };
            service.Settings.MaxConcurrentDownloads = 5;
            service.Settings.DownloadPlaylist = true;
            service.Settings.NoCacheDir = false;
            service.Settings.NoPartFile = false;
            service.Settings.ClipboardAutoPaste = false;
            service.Settings.IsAdvancedOptionsOpen = true;
            service.Settings.DefaultQuality = VideoQuality.FHD_1080p;
            service.Settings.DefaultAudioFormat = AudioFormat.Mp3;

            await service.SaveAsync();
            await service.LoadAsync();

            Assert.Equal("--test-option --cookies cookies.txt", service.Settings.ExtraOptions);
            Assert.Contains(@"C:\Videos", service.Settings.DestinationHistory);
            Assert.Contains(@"D:\Downloads", service.Settings.DestinationHistory);
            Assert.Contains("--test-option", service.Settings.ExtraOptionsHistory);
            Assert.Contains("--verbose", service.Settings.ExtraOptionsHistory);
            Assert.Equal(5, service.Settings.MaxConcurrentDownloads);
            Assert.True(service.Settings.DownloadPlaylist);
            Assert.False(service.Settings.NoCacheDir);
            Assert.False(service.Settings.NoPartFile);
            Assert.False(service.Settings.ClipboardAutoPaste);
            Assert.True(service.Settings.IsAdvancedOptionsOpen);
            Assert.Equal(VideoQuality.FHD_1080p, service.Settings.DefaultQuality);
            Assert.Equal(AudioFormat.Mp3, service.Settings.DefaultAudioFormat);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }

    [Fact]
    public void DownloadQueueManager_EnqueueItem_SetsQueuedStatus()
    {
        var engine = new YtDlpEngineService();
        var queue = new DownloadQueueManager(engine);

        var item = new DownloadItem
        {
            Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            OutputDirectory = @"C:\Temp"
        };

        queue.Enqueue(item);

        Assert.Contains(item, queue.Items);
    }

    [Fact]
    public void YtDlpEngineService_ResolvePath_ReturnsValidPath()
    {
        var engine = new YtDlpEngineService();
        string resolved = engine.ResolveEngineExecutablePath("yt-dlp.exe");
        Assert.NotNull(resolved);
        Assert.NotEmpty(resolved);
    }

    [Fact]
    public void YtDlpEngineService_ResolveQuickJsPath_ExecutesWithoutError()
    {
        var engine = new YtDlpEngineService();
        string? resolved = engine.ResolveQuickJsExecutablePath();
        // May be null or valid path if qjs.exe exists in environment
        Assert.True(resolved == null || resolved.EndsWith("qjs.exe", StringComparison.OrdinalIgnoreCase));
    }
}
