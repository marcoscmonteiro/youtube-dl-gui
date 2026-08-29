using System.IO;
using System.Text;
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

    public string StorageFolder => _storageFolder;
    public bool IsCloudSynced { get; }
    public AppSettings Settings { get; private set; } = AppSettings.CreateDefault();

    public JsonSettingsService(string? customStorageFolder = null)
    {
        if (!string.IsNullOrWhiteSpace(customStorageFolder))
        {
            _storageFolder = customStorageFolder;
            IsCloudSynced = false;
        }
        else
        {
            var (folder, isCloud) = ResolveStorageFolder();
            _storageFolder = folder;
            IsCloudSynced = isCloud;
        }

        _settingsFilePath = Path.Combine(_storageFolder, "settings.json");
        _historyFilePath = Path.Combine(_storageFolder, "history.json");

        if (string.IsNullOrWhiteSpace(customStorageFolder))
        {
            MigrateLegacySettingsIfNeeded();
        }
    }

    private static (string Path, bool IsCloud) ResolveStorageFolder()
    {
        // 1. Check OneDrive environment variables for automatic cloud sync & roaming
        string? oneDrive = Environment.GetEnvironmentVariable("OneDriveConsumer") 
                        ?? Environment.GetEnvironmentVariable("OneDrive") 
                        ?? Environment.GetEnvironmentVariable("OneDriveCommercial");

        if (!string.IsNullOrWhiteSpace(oneDrive) && Directory.Exists(oneDrive))
        {
            // Check existing directories to preserve current structure
            string aplicationsPath = Path.Combine(oneDrive, "Aplicativos", "YtDlpGui", "Config");
            if (Directory.Exists(aplicationsPath))
            {
                return (aplicationsPath, true);
            }

            string appsPath = Path.Combine(oneDrive, "Apps", "YtDlpGui", "Config");
            if (Directory.Exists(appsPath))
            {
                return (appsPath, true);
            }

            string rootYtDlp = Path.Combine(oneDrive, "YtDlpGui", "Config");
            if (Directory.Exists(rootYtDlp))
            {
                return (rootYtDlp, true);
            }

            // If none exist yet, select Apps or Aplicativos based on existing parents or OS culture
            if (Directory.Exists(Path.Combine(oneDrive, "Aplicativos")))
            {
                return (aplicationsPath, true);
            }

            if (Directory.Exists(Path.Combine(oneDrive, "Apps")))
            {
                return (appsPath, true);
            }

            string subFolder = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase)
                ? "Aplicativos"
                : "Apps";

            return (Path.Combine(oneDrive, subFolder, "YtDlpGui", "Config"), true);
        }

        // 2. Fallback to standard Windows Roaming %APPDATA%\YoutubeDlGui
        return (Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YoutubeDlGui"), false);
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
            string? json = await ReadTextSharedWithRecoveryAsync(_settingsFilePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    if (loaded != null)
                    {
                        Settings = loaded;
                        return;
                    }
                }
                catch (JsonException)
                {
                    // Primary JSON corrupted, attempt recovery from .bak
                    string backupFilePath = _settingsFilePath + ".bak";
                    if (File.Exists(backupFilePath))
                    {
                        string? bakJson = await ReadSharedAsync(backupFilePath);
                        if (!string.IsNullOrWhiteSpace(bakJson))
                        {
                            var backupLoaded = JsonSerializer.Deserialize<AppSettings>(bakJson, JsonOptions);
                            if (backupLoaded != null)
                            {
                                Settings = backupLoaded;
                                await SaveAsync(); // Restore healthy state to primary file
                                return;
                            }
                        }
                    }
                }
            }

            Settings = AppSettings.CreateDefault();
            await SaveAsync();
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
            string json = JsonSerializer.Serialize(Settings, JsonOptions);
            await WriteAtomicWithBackupAsync(_settingsFilePath, json);
        }
        catch { }
    }

    public async Task<List<DownloadItem>> LoadHistoryAsync()
    {
        try
        {
            string? json = await ReadTextSharedWithRecoveryAsync(_historyFilePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<DownloadItem>>(json, JsonOptions);
                    if (items != null)
                    {
                        return items;
                    }
                }
                catch (JsonException)
                {
                    string backupFilePath = _historyFilePath + ".bak";
                    if (File.Exists(backupFilePath))
                    {
                        string? bakJson = await ReadSharedAsync(backupFilePath);
                        if (!string.IsNullOrWhiteSpace(bakJson))
                        {
                            var backupItems = JsonSerializer.Deserialize<List<DownloadItem>>(bakJson, JsonOptions);
                            if (backupItems != null)
                            {
                                await SaveHistoryAsync(backupItems);
                                return backupItems;
                            }
                        }
                    }
                }
            }
        }
        catch { }

        return new List<DownloadItem>();
    }

    public async Task SaveHistoryAsync(IEnumerable<DownloadItem> items)
    {
        try
        {
            var list = items.ToList();
            string json = JsonSerializer.Serialize(list, JsonOptions);
            await WriteAtomicWithBackupAsync(_historyFilePath, json);
        }
        catch { }
    }

    private static async Task WriteAtomicWithBackupAsync(string targetFilePath, string content)
    {
        string dir = Path.GetDirectoryName(targetFilePath) ?? string.Empty;
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string tempFilePath = targetFilePath + ".tmp";
        string backupFilePath = targetFilePath + ".bak";

        const int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                // 1. Write content to temp file with immediate flush
                await using (var stream = new FileStream(
                    tempFilePath, 
                    FileMode.Create, 
                    FileAccess.Write, 
                    FileShare.ReadWrite, 
                    bufferSize: 4096, 
                    useAsync: true))
                await using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    await writer.WriteAsync(content);
                    await writer.FlushAsync();
                    await stream.FlushAsync();
                }

                // 2. Maintain .bak copy if target currently exists
                if (File.Exists(targetFilePath))
                {
                    try
                    {
                        File.Copy(targetFilePath, backupFilePath, overwrite: true);
                    }
                    catch
                    {
                        // Non-critical if backup copy fails due to lock
                    }
                }

                // 3. Atomically move/replace temp file to target file
                File.Move(tempFilePath, targetFilePath, overwrite: true);
                return;
            }
            catch (IOException) when (attempt < maxRetries - 1)
            {
                await Task.Delay(50 * (int)Math.Pow(2, attempt)); // Exponential backoff: 50ms, 100ms, 200ms, 400ms
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { }
                }
            }
        }
    }

    private static async Task<string?> ReadTextSharedWithRecoveryAsync(string targetFilePath)
    {
        string backupFilePath = targetFilePath + ".bak";

        if (File.Exists(targetFilePath))
        {
            string? content = await ReadSharedAsync(targetFilePath);
            if (!string.IsNullOrWhiteSpace(content))
            {
                return content;
            }
        }

        // Try fallback to .bak if target is missing, locked or empty
        if (File.Exists(backupFilePath))
        {
            string? backupContent = await ReadSharedAsync(backupFilePath);
            if (!string.IsNullOrWhiteSpace(backupContent))
            {
                return backupContent;
            }
        }

        return null;
    }

    private static async Task<string?> ReadSharedAsync(string filePath)
    {
        const int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                await using var stream = new FileStream(
                    filePath, 
                    FileMode.Open, 
                    FileAccess.Read, 
                    FileShare.ReadWrite, 
                    bufferSize: 4096, 
                    useAsync: true);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            catch (IOException) when (attempt < maxRetries - 1)
            {
                await Task.Delay(50 * (int)Math.Pow(2, attempt));
            }
            catch
            {
                break;
            }
        }
        return null;
    }
}
