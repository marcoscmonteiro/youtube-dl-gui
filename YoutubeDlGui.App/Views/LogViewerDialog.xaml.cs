using System.Windows;
using YoutubeDlGui.App.Services;
using YoutubeDlGui.App.ViewModels;

namespace YoutubeDlGui.App.Views;

public partial class LogViewerDialog : Window
{
    private readonly DownloadItemViewModel? _viewModel;

    public LogViewerDialog(DownloadItemViewModel viewModel)
    {
        InitializeComponent();
        ThemeManager.UpdateWindowTitleBarTheme(this);

        _viewModel = viewModel;
        DataContext = _viewModel;

        Title = $"Log em Tempo Real - {_viewModel.Title}";
        TextBoxLog.Text = string.IsNullOrEmpty(_viewModel.Log) ? "Aguardando início do processo..." : _viewModel.Log;

        if (CheckBoxAutoScroll.IsChecked == true && !string.IsNullOrEmpty(TextBoxLog.Text))
        {
            TextBoxLog.ScrollToEnd();
        }

        _viewModel.LogLineReceived += OnLogLineReceived;
        Closed += (s, e) =>
        {
            if (_viewModel != null)
            {
                _viewModel.LogLineReceived -= OnLogLineReceived;
            }
        };
    }

    public LogViewerDialog(string logText, string title)
    {
        InitializeComponent();
        ThemeManager.UpdateWindowTitleBarTheme(this);

        if (!string.IsNullOrEmpty(title))
        {
            Title = $"Log: {title}";
        }
        TextBoxLog.Text = string.IsNullOrEmpty(logText) ? "Nenhum log disponível." : logText;
        if (CheckBoxAutoScroll.IsChecked == true && !string.IsNullOrEmpty(TextBoxLog.Text))
        {
            TextBoxLog.ScrollToEnd();
        }
    }

    private void OnLogLineReceived(string line)
    {
        if (string.IsNullOrEmpty(line)) return;

        Dispatcher.InvokeAsync(() =>
        {
            if (string.IsNullOrEmpty(TextBoxLog.Text) || TextBoxLog.Text == "Aguardando início do processo..." || TextBoxLog.Text == "Nenhum log disponível.")
            {
                TextBoxLog.Text = _viewModel?.Log ?? line;
            }
            else
            {
                // Only append if not already present in the existing text
                TextBoxLog.AppendText(Environment.NewLine + line);
            }

            if (CheckBoxAutoScroll.IsChecked == true)
            {
                TextBoxLog.ScrollToEnd();
            }
        });
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e)
    {
        TextBoxLog.Text = string.Empty;
    }

    private void CopyLog_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(TextBoxLog.Text))
        {
            Clipboard.SetText(TextBoxLog.Text);
            MessageBox.Show("Log copiado para a área de transferência com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
