using System.Text.Json;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Services;

public class JsonSettingsService : ISettingsService
{
    private readonly string _appDataFolder;
    private readonly string _settingsFilePath;
    private readonly string _historyFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppSettings Settings { get; private set; } = AppSettings.CreateDefault();

    public JsonSettingsService()
    {
        _appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YoutubeDlGui");

        _settingsFilePath = Path.Combine(_appDataFolder, "settings.json");
        _historyFilePath = Path.Combine(_appDataFolder, "history.json");
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (loaded != null)
                {
                    Settings = loaded;
                }
            }
            else
            {
                Settings = AppSettings.CreateDefault();
                await SaveAsync();
            }
        }
        catch
        {
            Settings = AppSettings.CreateDefault();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            if (!Directory.Exists(_appDataFolder))
            {
                Directory.CreateDirectory(_appDataFolder);
            }

            string json = JsonSerializer.Serialize(Settings, JsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch { }
    }

    public async Task<List<DownloadItem>> LoadHistoryAsync()
    {
        try
        {
            if (File.Exists(_historyFilePath))
            {
                string json = await File.ReadAllTextAsync(_historyFilePath);
                var items = JsonSerializer.Deserialize<List<DownloadItem>>(json, JsonOptions);
                return items ?? new List<DownloadItem>();
            }
        }
        catch { }

        return new List<DownloadItem>();
    }

    public async Task SaveHistoryAsync(IEnumerable<DownloadItem> items)
    {
        try
        {
            if (!Directory.Exists(_appDataFolder))
            {
                Directory.CreateDirectory(_appDataFolder);
            }

            var list = items.ToList();
            string json = JsonSerializer.Serialize(list, JsonOptions);
            await File.WriteAllTextAsync(_historyFilePath, json);
        }
        catch { }
    }
}
