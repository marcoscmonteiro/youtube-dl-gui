using System.IO;
using System.Text.Json;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.Services;

public class JsonSettingsService : ISettingsService
{
    private readonly string _storageFolder;
    private readonly string _settingsFilePath;
    private readonly string _historyFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppSettings Settings { get; private set; } = AppSettings.CreateDefault();

    public JsonSettingsService(string? customStorageFolder = null)
    {
        _storageFolder = !string.IsNullOrWhiteSpace(customStorageFolder)
            ? customStorageFolder
            : ResolveStorageFolder();

        _settingsFilePath = Path.Combine(_storageFolder, "settings.json");
        _historyFilePath = Path.Combine(_storageFolder, "history.json");

        if (string.IsNullOrWhiteSpace(customStorageFolder))
        {
            MigrateLegacySettingsIfNeeded();
        }
    }

    private static string ResolveStorageFolder()
    {
        // 1. Try OneDrive consumer or commercial folders for automatic cloud sync & backup
        string? oneDrive = Environment.GetEnvironmentVariable("OneDriveConsumer") 
                        ?? Environment.GetEnvironmentVariable("OneDrive") 
                        ?? Environment.GetEnvironmentVariable("OneDriveCommercial");

        if (!string.IsNullOrWhiteSpace(oneDrive) && Directory.Exists(oneDrive))
        {
            return Path.Combine(oneDrive, "Aplicativos", "YtDlpGui", "Config");
        }

        // 2. Fallback to standard %APPDATA%\YoutubeDlGui
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YoutubeDlGui");
    }

    private void MigrateLegacySettingsIfNeeded()
    {
        try
        {
            string legacyAppDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "YoutubeDlGui");

            // If current storage is different from legacy %APPDATA% (e.g. OneDrive) and target doesn't exist yet
            if (!string.Equals(_storageFolder, legacyAppDataFolder, StringComparison.OrdinalIgnoreCase))
            {
                string legacySettings = Path.Combine(legacyAppDataFolder, "settings.json");
                string legacyHistory = Path.Combine(legacyAppDataFolder, "history.json");

                if (!Directory.Exists(_storageFolder))
                {
                    Directory.CreateDirectory(_storageFolder);
                }

                if (!File.Exists(_settingsFilePath) && File.Exists(legacySettings))
                {
                    File.Copy(legacySettings, _settingsFilePath, true);
                }

                if (!File.Exists(_historyFilePath) && File.Exists(legacyHistory))
                {
                    File.Copy(legacyHistory, _historyFilePath, true);
                }
            }
        }
        catch { }
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
            if (!Directory.Exists(_storageFolder))
            {
                Directory.CreateDirectory(_storageFolder);
            }

            string json = JsonSerializer.Serialize(Settings, JsonOptions);
            await WriteSafeWithRetryAsync(_settingsFilePath, json);
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
            if (!Directory.Exists(_storageFolder))
            {
                Directory.CreateDirectory(_storageFolder);
            }

            var list = items.ToList();
            string json = JsonSerializer.Serialize(list, JsonOptions);
            await WriteSafeWithRetryAsync(_historyFilePath, json);
        }
        catch { }
    }

    private static async Task WriteSafeWithRetryAsync(string filePath, string content)
    {
        const int maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, content);
                return;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                await Task.Delay(100);
            }
        }
    }
}
