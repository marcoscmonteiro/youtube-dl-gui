namespace YoutubeDlGui.Core.Models;

public class ExternalDownloadRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Quality { get; set; }
    public string? AudioFormat { get; set; }
    public bool? AudioOnly { get; set; }
    public bool? Playlist { get; set; }
    public string? ExtraOptions { get; set; }
    public string? DownloadDirectory { get; set; }
    public string? OutputDirectory { get; set; }
}
