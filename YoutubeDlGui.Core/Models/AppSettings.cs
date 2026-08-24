using YoutubeDlGui.Core.Enums;

namespace YoutubeDlGui.Core.Models;

public class AppSettings
{
    public string WorkDir { get; set; } = string.Empty;
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public string EngineExecutable { get; set; } = "yt-dlp.exe";
    public string ExtraOptions { get; set; } = string.Empty;
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

        return new AppSettings
        {
            WorkDir = defaultVideosPath,
            Theme = AppTheme.Dark,
            EngineExecutable = "yt-dlp.exe",
            ExtraOptions = string.Empty,
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
