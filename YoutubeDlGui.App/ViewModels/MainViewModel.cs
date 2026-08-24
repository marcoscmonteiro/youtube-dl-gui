using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using YoutubeDlGui.App.Services;
using YoutubeDlGui.App.Views;
using YoutubeDlGui.Core.Enums;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDownloadEngineService _engineService;
    private readonly IDownloadQueueManager _queueManager;
    private readonly ISettingsService _settingsService;
    private bool _isInitializing = false;

    [ObservableProperty]
    private string _urlInput = string.Empty;

    [ObservableProperty]
    private string _workDir = string.Empty;

    [ObservableProperty]
    private string _extraOptions = string.Empty;

    [ObservableProperty]
    private VideoQuality _selectedQuality = VideoQuality.Best;

    [ObservableProperty]
    private AudioFormat _selectedAudioFormat = AudioFormat.None;

    [ObservableProperty]
    private bool _downloadPlaylist = false;

    [ObservableProperty]
    private bool _noCacheDir = true;

    [ObservableProperty]
    private bool _noPartFile = true;

    [ObservableProperty]
    private bool _clipboardAutoPaste = true;

    [ObservableProperty]
    private int _maxConcurrentDownloads = 3;

    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private string _searchFilter = string.Empty;

    [ObservableProperty]
    private bool _isAdvancedOptionsOpen = false;

    [ObservableProperty]
    private DownloadItemViewModel? _selectedItem;

    public AppSettings Settings => _settingsService.Settings;

    public ObservableCollection<DownloadItemViewModel> Downloads { get; } = new();
    public ICollectionView FilteredDownloads { get; }

    public ObservableCollection<string> DestinationHistory { get; } = new();
    public ObservableCollection<string> ExtraOptionsHistory { get; } = new();

    public int TotalActiveCount => _queueManager.ActiveDownloadsCount;
    public int TotalQueuedCount => _queueManager.QueuedDownloadsCount;
    public int TotalCompletedCount => Downloads.Count(d => d.Status == DownloadStatus.Completed);
    public int TotalFailedCount => Downloads.Count(d => d.Status == DownloadStatus.Failed);

    public Array VideoQualityList => Enum.GetValues(typeof(VideoQuality));
    public Array AudioFormatList => Enum.GetValues(typeof(AudioFormat));

    public MainViewModel(
        IDownloadEngineService engineService,
        IDownloadQueueManager queueManager,
        ISettingsService settingsService)
    {
        _engineService = engineService;
        _queueManager = queueManager;
        _settingsService = settingsService;

        FilteredDownloads = CollectionViewSource.GetDefaultView(Downloads);
        FilteredDownloads.Filter = FilterDownloadItem;

        // Load persisted settings into ViewModel properties
        ApplySettingsToProperties(_settingsService.Settings);

        _queueManager.MaxConcurrentDownloads = MaxConcurrentDownloads;
        _queueManager.ItemStatusChanged += OnQueueItemStatusChanged;
        _queueManager.LogLineReceived += OnQueueLogLineReceived;

        _ = LoadSavedHistoryAsync();
    }

    private void ApplySettingsToProperties(AppSettings s)
    {
        _isInitializing = true;
        try
        {
            WorkDir = string.IsNullOrWhiteSpace(s.WorkDir) 
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) 
                : s.WorkDir;

            ExtraOptions = s.ExtraOptions ?? string.Empty;
            SelectedQuality = s.DefaultQuality;
            SelectedAudioFormat = s.DefaultAudioFormat;
            DownloadPlaylist = s.DownloadPlaylist;
            NoCacheDir = s.NoCacheDir;
            NoPartFile = s.NoPartFile;
            ClipboardAutoPaste = s.ClipboardAutoPaste;
            MaxConcurrentDownloads = s.MaxConcurrentDownloads > 0 ? s.MaxConcurrentDownloads : 3;
            IsDarkMode = s.Theme == AppTheme.Dark || (s.Theme == AppTheme.System && ThemeManager.IsSystemInDarkMode());
            IsAdvancedOptionsOpen = s.IsAdvancedOptionsOpen;

            DestinationHistory.Clear();
            if (s.DestinationHistory != null && s.DestinationHistory.Count > 0)
            {
                foreach (var d in s.DestinationHistory)
                {
                    if (!string.IsNullOrWhiteSpace(d) && !DestinationHistory.Contains(d))
                    {
                        DestinationHistory.Add(d);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(WorkDir) && !DestinationHistory.Contains(WorkDir))
            {
                DestinationHistory.Insert(0, WorkDir);
            }

            ExtraOptionsHistory.Clear();
            if (s.ExtraOptionsHistory != null && s.ExtraOptionsHistory.Count > 0)
            {
                foreach (var opt in s.ExtraOptionsHistory)
                {
                    if (!string.IsNullOrWhiteSpace(opt) && !ExtraOptionsHistory.Contains(opt))
                    {
                        ExtraOptionsHistory.Add(opt);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(ExtraOptions) && !ExtraOptionsHistory.Contains(ExtraOptions))
            {
                ExtraOptionsHistory.Insert(0, ExtraOptions);
            }
        }
        finally
        {
            _isInitializing = false;
        }
    }

    partial void OnSearchFilterChanged(string value)
    {
        FilteredDownloads.Refresh();
    }

    partial void OnMaxConcurrentDownloadsChanged(int value)
    {
        if (_isInitializing) return;
        _queueManager.MaxConcurrentDownloads = value > 0 ? value : 1;
        _settingsService.Settings.MaxConcurrentDownloads = value > 0 ? value : 1;
        _ = _settingsService.SaveAsync();
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        if (_isInitializing) return;
        var theme = value ? AppTheme.Dark : AppTheme.Light;
        ThemeManager.ApplyTheme(theme);
        _settingsService.Settings.Theme = theme;
        _ = _settingsService.SaveAsync();
    }

    partial void OnWorkDirChanged(string value) { if (!_isInitializing) SaveCurrentSettings(); }
    partial void OnExtraOptionsChanged(string value) { if (!_isInitializing) SaveCurrentSettings(); }
    partial void OnSelectedQualityChanged(VideoQuality value) { if (!_isInitializing) SaveCurrentSettings(); }
    partial void OnSelectedAudioFormatChanged(AudioFormat value) { if (!_isInitializing) SaveCurrentSettings(); }
    partial void OnDownloadPlaylistChanged(bool value) { if (!_isInitializing) SaveCurrentSettings(); }
    partial void OnNoCacheDirChanged(bool value) { if (!_isInitializing) SaveCurrentSettings(); }
    partial void OnNoPartFileChanged(bool value) { if (!_isInitializing) SaveCurrentSettings(); }
    partial void OnClipboardAutoPasteChanged(bool value) { if (!_isInitializing) SaveCurrentSettings(); }
    partial void OnIsAdvancedOptionsOpenChanged(bool value) { if (!_isInitializing) SaveCurrentSettings(); }

    private bool FilterDownloadItem(object obj)
    {
        if (obj is not DownloadItemViewModel item) return false;
        if (string.IsNullOrWhiteSpace(SearchFilter)) return true;

        string filter = SearchFilter.Trim();
        return (item.Title != null && item.Title.Contains(filter, StringComparison.OrdinalIgnoreCase))
            || (item.Url != null && item.Url.Contains(filter, StringComparison.OrdinalIgnoreCase))
            || (item.FileName != null && item.FileName.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }

    private void OnQueueItemStatusChanged(object? sender, DownloadItem model)
    {
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            var vm = Downloads.FirstOrDefault(d => d.Model.Id == model.Id);
            if (vm != null)
            {
                vm.UpdateFromModel();
            }

            OnPropertyChanged(nameof(TotalActiveCount));
            OnPropertyChanged(nameof(TotalQueuedCount));
            OnPropertyChanged(nameof(TotalCompletedCount));
            OnPropertyChanged(nameof(TotalFailedCount));

            _ = _settingsService.SaveHistoryAsync(Downloads.Select(d => d.Model));
        });
    }

    private void OnQueueLogLineReceived(object? sender, (DownloadItem Item, string LogLine) e)
    {
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            var vm = Downloads.FirstOrDefault(d => d.Model.Id == e.Item.Id);
            if (vm != null)
            {
                vm.AppendLogLine(e.LogLine);
            }
        });
    }

    private DownloadItemViewModel CreateDownloadItemViewModel(DownloadItem item)
    {
        return new DownloadItemViewModel(item, _queueManager, vm =>
        {
            Downloads.Remove(vm);
            _ = _settingsService.SaveHistoryAsync(Downloads.Select(d => d.Model));
            OnPropertyChanged(nameof(TotalActiveCount));
            OnPropertyChanged(nameof(TotalQueuedCount));
            OnPropertyChanged(nameof(TotalCompletedCount));
            OnPropertyChanged(nameof(TotalFailedCount));
        });
    }

    private async Task LoadSavedHistoryAsync()
    {
        var history = await _settingsService.LoadHistoryAsync();
        foreach (var item in history)
        {
            var vm = CreateDownloadItemViewModel(item);
            Downloads.Add(vm);
        }

        OnPropertyChanged(nameof(TotalCompletedCount));
        OnPropertyChanged(nameof(TotalFailedCount));
    }

    [RelayCommand]
    private void StartDownload()
    {
        string url = UrlInput.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show("Por favor, informe a URL do vídeo ou áudio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("A URL informada deve começar com http:// ou https://", "URL Inválida", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(WorkDir))
        {
            WorkDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        }

        string cmdArgs = BuildCommandLineArguments(url);

        var item = new DownloadItem
        {
            Url = url,
            Title = url,
            OutputDirectory = WorkDir,
            CommandLineArguments = cmdArgs,
            Status = DownloadStatus.Queued,
            CreatedAt = DateTime.Now
        };

        var vm = CreateDownloadItemViewModel(item);
        Downloads.Insert(0, vm);

        _queueManager.Enqueue(item);
        UrlInput = string.Empty;

        AddToHistory(DestinationHistory, WorkDir);
        if (!string.IsNullOrWhiteSpace(ExtraOptions))
        {
            AddToHistory(ExtraOptionsHistory, ExtraOptions);
        }

        SaveCurrentSettings();
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Selecione o diretório de download",
            InitialDirectory = Directory.Exists(WorkDir) ? WorkDir : Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
        };

        if (dialog.ShowDialog() == true)
        {
            WorkDir = dialog.FolderName;
            AddToHistory(DestinationHistory, WorkDir);
            SaveCurrentSettings();
        }
    }

    [RelayCommand]
    private void PasteFromClipboard()
    {
        string? text = ClipboardHelper.TryGetText()?.Trim();
        if (!string.IsNullOrEmpty(text))
        {
            UrlInput = text;
        }
    }

    [RelayCommand]
    private void ToggleAdvancedOptions()
    {
        IsAdvancedOptionsOpen = !IsAdvancedOptionsOpen;
    }

    [RelayCommand]
    private void ClearCompleted()
    {
        var toRemove = Downloads
            .Where(d => d.Status == DownloadStatus.Completed || d.Status == DownloadStatus.Cancelled)
            .ToList();

        foreach (var item in toRemove)
        {
            Downloads.Remove(item);
            _queueManager.Remove(item.Model);
        }

        _ = _settingsService.SaveHistoryAsync(Downloads.Select(d => d.Model));
        OnPropertyChanged(nameof(TotalCompletedCount));
    }

    [RelayCommand]
    private void CancelAll()
    {
        _queueManager.CancelAll();
    }

    [RelayCommand]
    private void OpenHelp()
    {
        var helpDialog = new HelpOptionsDialog(_engineService, _settingsService.Settings.EngineExecutable);
        helpDialog.Owner = System.Windows.Application.Current.MainWindow;
        helpDialog.ShowDialog();
    }

    [RelayCommand]
    private void UpdateEngine()
    {
        var updateDialog = new UpdateDialog(_engineService, _settingsService.Settings.EngineExecutable);
        updateDialog.Owner = System.Windows.Application.Current.MainWindow;
        updateDialog.ShowDialog();
    }

    [RelayCommand]
    private void ResetSettings()
    {
        var result = MessageBox.Show(
            "Deseja realmente restaurar todas as configurações e parâmetros da aplicação para os valores padrões de fábrica?",
            "Restaurar Padrões",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            var defaultSettings = AppSettings.CreateDefault();
            ApplySettingsToProperties(defaultSettings);
            SaveCurrentSettings();

            MessageBox.Show(
                "As configurações e parâmetros de download foram restaurados para os padrões originais com sucesso!",
                "Sucesso",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    public void OnWindowActivated()
    {
        if (ClipboardAutoPaste)
        {
            string? text = ClipboardHelper.TryGetText()?.Trim();
            if (!string.IsNullOrEmpty(text) &&
                (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                if (UrlInput != text)
                {
                    UrlInput = text;
                }
            }
        }
    }

    private string BuildCommandLineArguments(string url)
    {
        return BuildCustomCommandLineArguments(url, SelectedQuality, SelectedAudioFormat, DownloadPlaylist, ExtraOptions, null, NoCacheDir, NoPartFile);
    }

    public string BuildCustomCommandLineArguments(
        string url, 
        VideoQuality quality, 
        AudioFormat audioFormat, 
        bool downloadPlaylist, 
        string extraOptions, 
        string? cookieFilePath = null,
        bool noCacheDir = true,
        bool noPartFile = true,
        string? playerClients = null)
    {
        var sb = new StringBuilder();
        sb.Append("--encoding UTF8 --ignore-config ");

        // Always use QuickJS runtime if qjs.exe is present
        string? qjsPath = _engineService.ResolveQuickJsExecutablePath();
        if (!string.IsNullOrEmpty(qjsPath) && File.Exists(qjsPath))
        {
            sb.Append($"--js-runtimes \"quickjs:{qjsPath}\" ");
        }

        // Add custom player_client extractor-args if specifically requested/selected
        if (!string.IsNullOrWhiteSpace(playerClients) && 
            (string.IsNullOrWhiteSpace(extraOptions) || !extraOptions.Contains("player_client", StringComparison.OrdinalIgnoreCase)))
        {
            sb.Append($"--extractor-args \"youtube:player_client={playerClients.Trim()}\" ");
        }

        if (!string.IsNullOrWhiteSpace(extraOptions))
        {
            sb.Append(extraOptions.Trim()).Append(' ');
        }

        if (!string.IsNullOrWhiteSpace(cookieFilePath) && File.Exists(cookieFilePath))
        {
            sb.Append($"--cookies \"{cookieFilePath}\" ");
        }

        if (noCacheDir) sb.Append("--no-cache-dir ");
        if (!downloadPlaylist) sb.Append("--no-playlist ");
        if (noPartFile) sb.Append("--no-part ");

        if (audioFormat != AudioFormat.None)
        {
            sb.Append("-x ");
            if (audioFormat != AudioFormat.BestAudio)
            {
                sb.Append($"--audio-format {audioFormat.ToString().ToLowerInvariant()} ");
            }
            sb.Append("-f \"bestaudio/best\" ");
        }
        else
        {
            switch (quality)
            {
                case VideoQuality.UHD_4K:
                    sb.Append("-f \"bestvideo[height<=?2160]+bestaudio/best[height<=?2160]\" ");
                    break;
                case VideoQuality.QHD_1440p:
                    sb.Append("-f \"bestvideo[height<=?1440]+bestaudio/best[height<=?1440]\" ");
                    break;
                case VideoQuality.FHD_1080p:
                    sb.Append("-f \"bestvideo[height<=?1080]+bestaudio/best[height<=?1080]\" ");
                    break;
                case VideoQuality.HD_720p:
                    sb.Append("-f \"bestvideo[height<=?720]+bestaudio/best[height<=?720]\" ");
                    break;
                case VideoQuality.SD_480p:
                    sb.Append("-f \"bestvideo[height<=?480]+bestaudio/best[height<=?480]\" ");
                    break;
                case VideoQuality.Worst:
                    sb.Append("-f \"worstvideo+worstaudio/worst\" ");
                    break;
                default:
                    // Best
                    sb.Append("-f \"bestvideo+bestaudio/best\" ");
                    break;
            }
        }

        sb.Append($"\"{url}\"");
        return sb.ToString();
    }

    public void EnqueueFromExternal(ExternalDownloadRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Url)) return;
        string url = req.Url.Trim();
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // 1. Output directory: strictly from extension, or fallback to standard Downloads folder (ignores UI WorkDir)
        string targetDirectory;
        string? requestedDir = req.DownloadDirectory ?? req.OutputDirectory;
        if (!string.IsNullOrWhiteSpace(requestedDir))
        {
            try
            {
                string expandedDir = Environment.ExpandEnvironmentVariables(requestedDir.Trim());
                if (!Directory.Exists(expandedDir))
                {
                    Directory.CreateDirectory(expandedDir);
                }
                targetDirectory = expandedDir;
            }
            catch
            {
                targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
        }
        else
        {
            targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (!Directory.Exists(targetDirectory))
            {
                targetDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            }
        }

        // 2. Video quality: strictly from extension (default VideoQuality.Best) - IGNORE application UI state
        VideoQuality quality = VideoQuality.Best;
        if (!string.IsNullOrWhiteSpace(req.Quality) && Enum.TryParse<VideoQuality>(req.Quality, true, out var parsedQuality))
        {
            quality = parsedQuality;
        }

        // 3. Audio format: strictly from extension (default AudioFormat.None = video) - IGNORE application UI state
        AudioFormat audioFormat = AudioFormat.None;
        if (req.AudioOnly == true)
        {
            audioFormat = AudioFormat.Mp3;
            if (!string.IsNullOrWhiteSpace(req.AudioFormat) && 
                !req.AudioFormat.Equals("None", StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse<AudioFormat>(req.AudioFormat, true, out var parsedAudio))
            {
                audioFormat = parsedAudio;
            }
        }
        else if (!string.IsNullOrWhiteSpace(req.AudioFormat) && 
                 !req.AudioFormat.Equals("None", StringComparison.OrdinalIgnoreCase) &&
                 Enum.TryParse<AudioFormat>(req.AudioFormat, true, out var directAudio))
        {
            audioFormat = directAudio;
        }

        // 4. Playlist: strictly from extension (default false) - IGNORE application UI state
        bool playlist = req.Playlist ?? false;

        // 5. Extra options: strictly from extension (default empty) - IGNORE application UI state
        string extraOpts = req.ExtraOptions ?? string.Empty;

        // 6. Cookies: strictly from extension
        string? tempCookiePath = null;
        if (!string.IsNullOrWhiteSpace(req.CookiesText))
        {
            try
            {
                string cleanCookies = req.CookiesText.TrimStart('\uFEFF', '\r', '\n', ' ').TrimEnd();
                if (!cleanCookies.StartsWith("# Netscape HTTP Cookie File", StringComparison.OrdinalIgnoreCase) &&
                    !cleanCookies.StartsWith("# HTTP Cookie File", StringComparison.OrdinalIgnoreCase))
                {
                    cleanCookies = "# Netscape HTTP Cookie File\n" + cleanCookies;
                }

                tempCookiePath = Path.Combine(Path.GetTempPath(), $"ydl_cookie_{Guid.NewGuid():N}.txt");
                var utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                File.WriteAllText(tempCookiePath, cleanCookies + "\n", utf8WithoutBom);
            }
            catch
            {
                tempCookiePath = null;
            }
        }

        // 7. Extract player_clients from request if provided
        string? playerClients = req.PlayerClients;
        if (string.IsNullOrWhiteSpace(playerClients) && !string.IsNullOrWhiteSpace(req.ExtractorArgs))
        {
            if (req.ExtractorArgs.StartsWith("youtube:player_client=", StringComparison.OrdinalIgnoreCase))
            {
                playerClients = req.ExtractorArgs.Substring("youtube:player_client=".Length);
            }
        }

        // 8. Build command line arguments without using UI state
        string cmdArgs = BuildCustomCommandLineArguments(
            url: url,
            quality: quality,
            audioFormat: audioFormat,
            downloadPlaylist: playlist,
            extraOptions: extraOpts,
            cookieFilePath: tempCookiePath,
            noCacheDir: true,
            noPartFile: true,
            playerClients: playerClients
        );

        var item = new DownloadItem
        {
            Url = url,
            Title = !string.IsNullOrWhiteSpace(req.Title) ? req.Title : url,
            OutputDirectory = targetDirectory,
            CommandLineArguments = cmdArgs,
            Status = DownloadStatus.Queued,
            CreatedAt = DateTime.Now,
            TemporaryCookieFilePath = tempCookiePath
        };

        var vm = CreateDownloadItemViewModel(item);
        Downloads.Insert(0, vm);

        _queueManager.Enqueue(item);
    }

    public object GetStatusSummary()
    {
        string defaultDownloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        return new
        {
            active = TotalActiveCount,
            queued = TotalQueuedCount,
            completed = TotalCompletedCount,
            failed = TotalFailedCount,
            total = Downloads.Count,
            workDir = WorkDir,
            defaultDownloadsFolder = Directory.Exists(defaultDownloadsFolder) ? defaultDownloadsFolder : WorkDir,
            defaultQuality = SelectedQuality.ToString(),
            defaultAudioFormat = SelectedAudioFormat.ToString()
        };
    }

    public void SaveWindowPlacement(double width, double height, double? top, double? left)
    {
        var s = _settingsService.Settings;
        s.WindowWidth = width;
        s.WindowHeight = height;
        s.WindowTop = top;
        s.WindowLeft = left;
        _ = _settingsService.SaveAsync();
    }

    public void SaveCurrentSettings()
    {
        var s = _settingsService.Settings;
        s.WorkDir = WorkDir;
        s.DestinationHistory = DestinationHistory.ToList();
        s.ExtraOptions = ExtraOptions;
        s.ExtraOptionsHistory = ExtraOptionsHistory.ToList();
        s.DefaultQuality = SelectedQuality;
        s.DefaultAudioFormat = SelectedAudioFormat;
        s.DownloadPlaylist = DownloadPlaylist;
        s.NoCacheDir = NoCacheDir;
        s.NoPartFile = NoPartFile;
        s.ClipboardAutoPaste = ClipboardAutoPaste;
        s.MaxConcurrentDownloads = MaxConcurrentDownloads;
        s.Theme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        s.IsAdvancedOptionsOpen = IsAdvancedOptionsOpen;

        _ = _settingsService.SaveAsync();
    }

    private static void AddToHistory(ObservableCollection<string> collection, string value, int maxItems = 15)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        string trimmed = value.Trim();

        for (int i = collection.Count - 1; i >= 0; i--)
        {
            if (string.Equals(collection[i], trimmed, StringComparison.OrdinalIgnoreCase))
            {
                collection.RemoveAt(i);
            }
        }

        collection.Insert(0, trimmed);

        while (collection.Count > maxItems)
        {
            collection.RemoveAt(collection.Count - 1);
        }
    }
}
