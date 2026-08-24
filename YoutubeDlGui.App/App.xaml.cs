using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDlGui.App.Services;
using YoutubeDlGui.App.ViewModels;
using YoutubeDlGui.App.Views;
using YoutubeDlGui.Core.Interfaces;
using YoutubeDlGui.Core.Models;
using YoutubeDlGui.Services;

namespace YoutubeDlGui.App;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    private ISingleInstanceService? _singleInstanceService;
    private IHttpBridgeService? _httpBridgeService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceService = new SingleInstanceService();

        if (!_singleInstanceService.IsFirstInstance)
        {
            if (e.Args.Length > 0)
            {
                await _singleInstanceService.SendArgsToFirstInstanceAsync(e.Args);
            }
            Shutdown();
            return;
        }

        var services = new ServiceCollection();

        // Core Services
        services.AddSingleton<ISingleInstanceService>(_singleInstanceService);
        services.AddSingleton<ISettingsService, JsonSettingsService>();
        services.AddSingleton<IDownloadEngineService, YtDlpEngineService>();
        services.AddSingleton<IDownloadQueueManager, DownloadQueueManager>();

        // ViewModels
        services.AddSingleton<MainViewModel>();

        // HTTP Bridge Service (resolves MainViewModel dynamically for status summary)
        services.AddSingleton<IHttpBridgeService>(sp =>
        {
            return new HttpBridgeService(() =>
            {
                var vm = sp.GetRequiredService<MainViewModel>();
                return vm.GetStatusSummary();
            });
        });

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
        var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        // 4. Setup Single Instance listening for external arguments
        _singleInstanceService.ArgumentsReceived += (s, args) =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                ProcessExternalArguments(args, mainViewModel, mainWindow);
            });
        };
        _singleInstanceService.StartListening();

        // 5. Setup and start HTTP Bridge for browser extension integration
        _httpBridgeService = ServiceProvider.GetRequiredService<IHttpBridgeService>();
        _httpBridgeService.DownloadRequested += (s, req) =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                mainViewModel.EnqueueFromExternal(req);
                BringWindowToFront(mainWindow);
            });
        };

        if (settingsService.Settings.EnableBrowserIntegration)
        {
            try
            {
                await _httpBridgeService.StartAsync(settingsService.Settings.BridgePort);
            }
            catch
            {
                // Fallback port or log if port is occupied
            }
        }

        // 6. Process any initial command-line arguments
        if (e.Args.Length > 0)
        {
            ProcessExternalArguments(e.Args, mainViewModel, mainWindow);
        }
    }

    private static void ProcessExternalArguments(string[] args, MainViewModel mainViewModel, MainWindow mainWindow)
    {
        if (args.Length == 0) return;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].Trim();
            if (string.IsNullOrEmpty(arg)) continue;

            // Handle custom protocol ydlgui:// or youtubedl-gui://
            if (arg.StartsWith("ydlgui://", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("youtubedl-gui://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(arg);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                string? url = queryParams["url"] ?? queryParams["u"];
                if (!string.IsNullOrWhiteSpace(url))
                {
                    mainViewModel.EnqueueFromExternal(new ExternalDownloadRequest
                    {
                        Url = url,
                        Quality = queryParams["quality"],
                        AudioFormat = queryParams["audio"],
                        AudioOnly = queryParams["audioOnly"] == "true",
                        Playlist = queryParams["playlist"] == "true",
                        DownloadDirectory = queryParams["dir"] ?? queryParams["downloadDirectory"] ?? queryParams["outDir"]
                    });
                    BringWindowToFront(mainWindow);
                }
                continue;
            }

            // Handle --url <url> [optional --dir <dir>]
            if (arg.Equals("--url", StringComparison.OrdinalIgnoreCase) || arg.Equals("-u", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    string targetUrl = args[++i].Trim();
                    string? customDir = null;
                    if (i + 2 < args.Length && (args[i + 1].Equals("--dir", StringComparison.OrdinalIgnoreCase) || args[i + 1].Equals("-d", StringComparison.OrdinalIgnoreCase)))
                    {
                        i++;
                        customDir = args[++i].Trim();
                    }

                    mainViewModel.EnqueueFromExternal(new ExternalDownloadRequest 
                    { 
                        Url = targetUrl,
                        DownloadDirectory = customDir
                    });
                    BringWindowToFront(mainWindow);
                }
                continue;
            }

            // Handle direct URL
            if (arg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                arg.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                mainViewModel.EnqueueFromExternal(new ExternalDownloadRequest { Url = arg });
                BringWindowToFront(mainWindow);
            }
        }
    }

    private static void BringWindowToFront(MainWindow window)
    {
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }
        window.Activate();
        window.Topmost = true;
        window.Topmost = false;
        window.Focus();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _httpBridgeService?.Dispose();
        _singleInstanceService?.Dispose();
        base.OnExit(e);
    }
}
