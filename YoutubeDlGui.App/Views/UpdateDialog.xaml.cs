using System.Windows;
using YoutubeDlGui.App.Services;
using YoutubeDlGui.Core.Interfaces;

namespace YoutubeDlGui.App.Views;

public partial class UpdateDialog : Window
{
    private readonly IDownloadEngineService _engineService;
    private readonly string _engineExecutable;

    public UpdateDialog(IDownloadEngineService engineService, string engineExecutable)
    {
        InitializeComponent();
        ThemeManager.UpdateWindowTitleBarTheme(this);

        _engineService = engineService;
        _engineExecutable = engineExecutable;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        TextBoxOutput.Text = "Iniciando verificação de atualizações...\n";

        var progress = new Progress<string>(line =>
        {
            TextBoxOutput.Text += line + "\n";
            TextBoxOutput.ScrollToEnd();
        });

        try
        {
            await _engineService.UpdateEngineAsync(_engineExecutable, progress);
            TextBoxOutput.Text += "\nProcesso de atualização finalizado.";
        }
        catch (Exception ex)
        {
            TextBoxOutput.Text += $"\nErro durante a atualização: {ex.Message}";
        }
        finally
        {
            ProgressBarUpdate.IsIndeterminate = false;
            ProgressBarUpdate.Value = 100;
            ButtonClose.IsEnabled = true;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
