using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDlGui.App.Services;
using YoutubeDlGui.App.ViewModels;
using YoutubeDlGui.App.Views;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Services;

namespace YoutubeDlGui.App;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Core Services
        services.AddSingleton<ISettingsService, JsonSettingsService>();
        services.AddSingleton<IDownloadEngineService, YtDlpEngineService>();
        services.AddSingleton<IDownloadQueueManager, DownloadQueueManager>();

        // ViewModels
        services.AddSingleton<MainViewModel>();

        // Views
        services.AddTransient<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();

        // 1. Explicitly load settings from disk before creating UI
        var settingsService = ServiceProvider.GetRequiredService<ISettingsService>();
        await settingsService.LoadAsync();

        // 2. Apply saved theme
        ThemeManager.ApplyTheme(settingsService.Settings.Theme);

        // 3. Instantiate and show MainWindow with loaded settings
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
