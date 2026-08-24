namespace YoutubeDlGui.Core.Enums;

public enum DownloadStatus
{
    Queued,
    Downloading,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public enum AppTheme
{
    System,
    Dark,
    Light
}

public enum VideoQuality
{
    Best = 0,
    UHD_4K = 1,
    QHD_1440p = 2,
    FHD_1080p = 3,
    HD_720p = 4,
    SD_480p = 5,
    Worst = 6
}

public enum AudioFormat
{
    None = 0,
    BestAudio = 1,
    Mp3 = 2,
    Aac = 3,
    M4a = 4,
    Opus = 5,
    Vorbis = 6,
    Flac = 7,
    Wav = 8
}
