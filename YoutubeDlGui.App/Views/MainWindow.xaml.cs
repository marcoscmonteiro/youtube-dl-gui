using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDlGui.App.Services;
using YoutubeDlGui.App.ViewModels;

namespace YoutubeDlGui.App.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
        DataContext = _viewModel;

        RestoreWindowPlacement();
        ThemeManager.UpdateWindowTitleBarTheme(this);

        Activated += MainWindow_Activated;
        Closing += MainWindow_Closing;
        StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            PathMaximize.Data = System.Windows.Media.Geometry.Parse("M 2 0 L 10 0 L 10 8 M 0 2 L 8 2 L 8 10 L 0 10 Z");
            ButtonMaximize.ToolTip = "Restaurar";
            RootGrid.Margin = new Thickness(6);
        }
        else
        {
            PathMaximize.Data = System.Windows.Media.Geometry.Parse("M 0 0 L 10 0 L 10 10 L 0 10 Z");
            ButtonMaximize.ToolTip = "Maximizar";
            RootGrid.Margin = new Thickness(0);
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            SystemCommands.RestoreWindow(this);
        }
        else
        {
            SystemCommands.MaximizeWindow(this);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void RestoreWindowPlacement()
    {
        var s = _viewModel.Settings;
        if (s.WindowWidth > 400) Width = s.WindowWidth;
        if (s.WindowHeight > 300) Height = s.WindowHeight;

        if (s.WindowTop.HasValue && s.WindowLeft.HasValue && s.WindowTop > 0 && s.WindowLeft > 0)
        {
            // Verify within virtual screen bounds
            if (s.WindowLeft < SystemParameters.VirtualScreenWidth && s.WindowTop < SystemParameters.VirtualScreenHeight)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Top = s.WindowTop.Value;
                Left = s.WindowLeft.Value;
            }
        }
    }

    private void MainWindow_Activated(object? sender, EventArgs e)
    {
        string prev = _viewModel.UrlInput;
        _viewModel.OnWindowActivated();
        if (!string.IsNullOrEmpty(_viewModel.UrlInput) && _viewModel.UrlInput != prev)
        {
            TextBoxUrl.Focus();
            TextBoxUrl.SelectAll();
        }
    }

    private void TextBoxUrl_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.StartDownloadCommand.Execute(null);
        }
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_viewModel.TotalActiveCount > 0 || _viewModel.TotalQueuedCount > 0)
        {
            var result = MessageBox.Show(
                "Existem downloads em andamento ou na fila.\nFechar o aplicativo cancelará todos os downloads.\n\nDeseja realmente fechar?",
                "Confirmar Saída",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            _viewModel.CancelAllCommand.Execute(null);
        }

        if (WindowState == WindowState.Normal)
        {
            _viewModel.SaveWindowPlacement(Width, Height, Top, Left);
        }

        _viewModel.SaveCurrentSettings();
    }
}
