using System.Text.Json.Serialization;
using YoutubeDlGui.Core.Enums;

namespace YoutubeDlGui.Core.Models;

public class DownloadItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public string CommandLineArguments { get; set; } = string.Empty;
    public DownloadStatus Status { get; set; } = DownloadStatus.Queued;
    public double ProgressPercentage { get; set; }
    public string DownloadSpeed { get; set; } = string.Empty;
    public string Eta { get; set; } = string.Empty;
    public string TotalSize { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = "Queued";
    public string Log { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }

    [JsonIgnore]
    public string FullPath => !string.IsNullOrEmpty(OutputDirectory) && !string.IsNullOrEmpty(FileName)
        ? Path.Combine(OutputDirectory, FileName)
        : string.Empty;

    [JsonIgnore]
    public string PartFullPath => !string.IsNullOrEmpty(FullPath)
        ? FullPath + ".part"
        : string.Empty;

    [JsonIgnore]
    public bool FileExists => !string.IsNullOrEmpty(FullPath) && File.Exists(FullPath);

    [JsonIgnore]
    public bool PartFileExists => !string.IsNullOrEmpty(PartFullPath) && File.Exists(PartFullPath);
}
