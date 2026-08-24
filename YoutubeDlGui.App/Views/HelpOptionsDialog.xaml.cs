using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using YoutubeDlGui.App.Services;
using YoutubeDlGui.Core.Interfaces;

namespace YoutubeDlGui.App.Views;

public partial class HelpOptionsDialog : Window
{
    private readonly IDownloadEngineService _engineService;
    private readonly string _engineExecutable;

    public HelpOptionsDialog(IDownloadEngineService engineService, string engineExecutable)
    {
        InitializeComponent();
        ThemeManager.UpdateWindowTitleBarTheme(this);

        _engineService = engineService;
        _engineExecutable = engineExecutable;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        TextBoxHelp.Text = "Carregando opções de ajuda da engine...";
        try
        {
            string helpText = await _engineService.GetHelpAsync(_engineExecutable);
            TextBoxHelp.Text = helpText;
        }
        catch (Exception ex)
        {
            TextBoxHelp.Text = $"Erro ao obter ajuda da engine: {ex.Message}";
        }
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        PerformSearch();
    }

    private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            PerformSearch();
        }
    }

    private void PerformSearch()
    {
        string query = TextBoxSearch.Text;
        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(TextBoxHelp.Text)) return;

        int startIndex = TextBoxHelp.SelectionStart + TextBoxHelp.SelectionLength;
        if (startIndex >= TextBoxHelp.Text.Length) startIndex = 0;

        int index = TextBoxHelp.Text.IndexOf(query, startIndex, StringComparison.OrdinalIgnoreCase);
        if (index == -1 && startIndex > 0)
        {
            index = TextBoxHelp.Text.IndexOf(query, 0, StringComparison.OrdinalIgnoreCase);
        }

        if (index != -1)
        {
            TextBoxHelp.Focus();
            TextBoxHelp.Select(index, query.Length);
            TextBoxHelp.ScrollToLine(TextBoxHelp.GetLineIndexFromCharacterIndex(index));
        }
        else
        {
            MessageBox.Show($"Termo \"{query}\" não encontrado.", "Busca", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch { }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
