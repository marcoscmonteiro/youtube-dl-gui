using System.Windows;
using YoutubeDlGui.App.Services;

namespace YoutubeDlGui.App.Views;

public partial class LogViewerDialog : Window
{
    public LogViewerDialog(string logText, string title)
    {
        InitializeComponent();
        ThemeManager.UpdateWindowTitleBarTheme(this);

        if (!string.IsNullOrEmpty(title))
        {
            TextTitle.Text = $"Log: {title}";
        }
        TextBoxLog.Text = string.IsNullOrEmpty(logText) ? "Nenhum log disponível." : logText;
    }

    private void CopyLog_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(TextBoxLog.Text))
        {
            Clipboard.SetText(TextBoxLog.Text);
            MessageBox.Show("Log copiado para a área de transferência.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
