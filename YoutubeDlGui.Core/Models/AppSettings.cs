using System.IO;
using YoutubeDlGui.Core.Enums;

namespace YoutubeDlGui.Core.Models;

public class AppSettings
{
    public string WorkDir { get; set; } = string.Empty;
    public List<string> DestinationHistory { get; set; } = new();
    public string ExtraOptions { get; set; } = string.Empty;
    public List<string> ExtraOptionsHistory { get; set; } = new();
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public string EngineExecutable { get; set; } = "yt-dlp.exe";
    public bool ClipboardAutoPaste { get; set; } = true;
    public int MaxConcurrentDownloads { get; set; } = 3;
    public VideoQuality DefaultQuality { get; set; } = VideoQuality.Best;
    public AudioFormat DefaultAudioFormat { get; set; } = AudioFormat.None;
    public bool DownloadPlaylist { get; set; } = false;
    public bool NoCacheDir { get; set; } = true;
    public bool NoPartFile { get; set; } = true;
    public bool UseFfplay { get; set; } = false;
    public bool IsAdvancedOptionsOpen { get; set; } = false;
    public int BridgePort { get; set; } = 48190;
    public bool EnableBrowserIntegration { get; set; } = true;

    public double WindowWidth { get; set; } = 1040;
    public double WindowHeight { get; set; } = 720;
    public double? WindowTop { get; set; }
    public double? WindowLeft { get; set; }

    public static AppSettings CreateDefault()
    {
        string defaultVideosPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        if (string.IsNullOrEmpty(defaultVideosPath))
        {
            defaultVideosPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        var defaultDestinations = new List<string>();
        if (!string.IsNullOrEmpty(defaultVideosPath))
        {
            defaultDestinations.Add(defaultVideosPath);
        }

        string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        if (Directory.Exists(downloadsFolder) && !defaultDestinations.Contains(downloadsFolder, StringComparer.OrdinalIgnoreCase))
        {
            defaultDestinations.Add(downloadsFolder);
        }

        return new AppSettings
        {
            WorkDir = defaultVideosPath,
            DestinationHistory = defaultDestinations,
            ExtraOptions = string.Empty,
            ExtraOptionsHistory = new List<string>(),
            Theme = AppTheme.Dark,
            EngineExecutable = "yt-dlp.exe",
            DefaultQuality = VideoQuality.Best,
            DefaultAudioFormat = AudioFormat.None,
            MaxConcurrentDownloads = 3,
            ClipboardAutoPaste = true,
            DownloadPlaylist = false,
            NoCacheDir = true,
            NoPartFile = true,
            IsAdvancedOptionsOpen = false,
            WindowWidth = 1040,
            WindowHeight = 720
        };
    }
}
