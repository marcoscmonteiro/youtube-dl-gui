namespace YoutubeDlGui.Core.Models;

public class DownloadProgressReport
{
    public double Percentage { get; set; }
    public string Speed { get; set; } = string.Empty;
    public string Eta { get; set; } = string.Empty;
    public string TotalSize { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string? ExtractedFileName { get; set; }
    public string RawLogLine { get; set; } = string.Empty;
}
