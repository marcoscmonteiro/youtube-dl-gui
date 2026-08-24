using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDlGui.App.Views;
using YoutubeDlGui.Core.Enums;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Core.Models;

namespace YoutubeDlGui.App.ViewModels;

public partial class DownloadItemViewModel : ObservableObject
{
    private readonly IDownloadQueueManager _queueManager;
    private readonly Action<DownloadItemViewModel>? _onRemove;

    public DownloadItem Model { get; }

    [ObservableProperty]
    private string _url;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _fileName;

    [ObservableProperty]
    private string _outputDirectory;

    [ObservableProperty]
    private DownloadStatus _status;

    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private string _downloadSpeed;

    [ObservableProperty]
    private string _eta;

    [ObservableProperty]
    private string _totalSize;

    [ObservableProperty]
    private string _statusMessage;

    [ObservableProperty]
    private string _log;

    public bool CanCancel => Status == DownloadStatus.Downloading || Status == DownloadStatus.Queued || Status == DownloadStatus.Processing;
    public bool CanRetry => Status == DownloadStatus.Failed || Status == DownloadStatus.Cancelled || Status == DownloadStatus.Completed;
    public bool CanPlay => HasDownloadedFile;
    public bool HasDownloadedFile => Model.FileExists || Model.PartFileExists;
    public bool CanOpenFolder => !string.IsNullOrEmpty(Model.OutputDirectory) && Directory.Exists(Model.OutputDirectory);

    public DownloadItemViewModel(DownloadItem model, IDownloadQueueManager queueManager, Action<DownloadItemViewModel>? onRemove = null)
    {
        Model = model;
        _queueManager = queueManager;
        _onRemove = onRemove;

        _url = model.Url;
        _title = string.IsNullOrEmpty(model.Title) ? model.Url : model.Title;
        _fileName = model.FileName;
        _outputDirectory = model.OutputDirectory;
        _status = model.Status;
        _progressPercentage = model.ProgressPercentage;
        _downloadSpeed = model.DownloadSpeed;
        _eta = model.Eta;
        _totalSize = model.TotalSize;
        _statusMessage = model.StatusMessage;
        _log = model.Log;
    }

    public void UpdateFromModel()
    {
        Url = Model.Url;
        if (!string.IsNullOrEmpty(Model.Title)) Title = Model.Title;
        FileName = Model.FileName;
        OutputDirectory = Model.OutputDirectory;
        Status = Model.Status;
        ProgressPercentage = Model.ProgressPercentage;
        DownloadSpeed = Model.DownloadSpeed;
        Eta = Model.Eta;
        TotalSize = Model.TotalSize;
        StatusMessage = Model.StatusMessage;
        Log = Model.Log;

        OnPropertyChanged(nameof(CanCancel));
        OnPropertyChanged(nameof(CanRetry));
        OnPropertyChanged(nameof(CanPlay));
        OnPropertyChanged(nameof(HasDownloadedFile));
        OnPropertyChanged(nameof(CanOpenFolder));
    }

    [RelayCommand]
    private void Cancel()
    {
        _queueManager.Cancel(Model);
        UpdateFromModel();
    }

    [RelayCommand]
    private void Retry()
    {
        _queueManager.Retry(Model);
        UpdateFromModel();
    }

    [RelayCommand]
    private void Remove()
    {
        _queueManager.Remove(Model);
        _onRemove?.Invoke(this);
    }

    [RelayCommand]
    private void OpenFolder()
    {
        string path = Model.FileExists ? Model.FullPath : (Model.PartFileExists ? Model.PartFullPath : Model.OutputDirectory);
        if (File.Exists(path))
        {
            Process.Start("explorer.exe", $"/select,\"{path}\"");
        }
        else if (Directory.Exists(Model.OutputDirectory))
        {
            Process.Start("explorer.exe", $"\"{Model.OutputDirectory}\"");
        }
    }

    [RelayCommand]
    private void Play()
    {
        string fileToPlay = Model.FileExists ? Model.FullPath : (Model.PartFileExists ? Model.PartFullPath : string.Empty);
        if (!string.IsNullOrEmpty(fileToPlay) && File.Exists(fileToPlay))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileToPlay,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível reproduzir o arquivo:\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void CopyUrl()
    {
        if (!string.IsNullOrEmpty(Url))
        {
            Clipboard.SetText(Url);
        }
    }

    [RelayCommand]
    private void OpenInBrowser()
    {
        if (!string.IsNullOrEmpty(Url) && (Url.StartsWith("http://") || Url.StartsWith("https://")))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Url,
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }

    [RelayCommand]
    private void ViewLog()
    {
        var logWindow = new LogViewerDialog(Log, Title);
        logWindow.Owner = System.Windows.Application.Current.MainWindow;
        logWindow.ShowDialog();
    }

    [RelayCommand]
    private void DeleteFile()
    {
        string targetFile = Model.FileExists ? Model.FullPath : (Model.PartFileExists ? Model.PartFullPath : FileName);
        var result = MessageBox.Show(
            $"Deseja realmente excluir o arquivo do disco?\n{targetFile}",
            "Confirmar Exclusão de Arquivo",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                if (Model.PartFileExists) File.Delete(Model.PartFullPath);
                if (Model.FileExists) File.Delete(Model.FullPath);
                UpdateFromModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir arquivo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
