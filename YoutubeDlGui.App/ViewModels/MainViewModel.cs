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

    private async Task LoadSavedHistoryAsync()
    {
        var history = await _settingsService.LoadHistoryAsync();
        foreach (var item in history)
        {
            var vm = new DownloadItemViewModel(item, _queueManager);
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

        var vm = new DownloadItemViewModel(item, _queueManager);
        Downloads.Insert(0, vm);

        _queueManager.Enqueue(item);
        UrlInput = string.Empty;

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
        var sb = new StringBuilder();
        sb.Append("--encoding UTF8 --ignore-config ");

        if (!string.IsNullOrWhiteSpace(ExtraOptions))
        {
            sb.Append(ExtraOptions.Trim()).Append(' ');
        }

        if (NoCacheDir) sb.Append("--no-cache-dir ");
        if (!DownloadPlaylist) sb.Append("--no-playlist ");
        if (NoPartFile) sb.Append("--no-part ");

        if (SelectedAudioFormat != AudioFormat.None)
        {
            sb.Append("-x ");
            if (SelectedAudioFormat != AudioFormat.BestAudio)
            {
                sb.Append($"--audio-format {SelectedAudioFormat.ToString().ToLowerInvariant()} ");
            }
            sb.Append("-f \"bestaudio/best\" ");
        }
        else
        {
            switch (SelectedQuality)
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
        s.ExtraOptions = ExtraOptions;
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
}
