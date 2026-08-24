using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using YoutubeDlGui.Core.Enums;

namespace YoutubeDlGui.App.Services;

public static class ThemeManager
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    public static void ApplyTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        bool isDark = theme switch
        {
            AppTheme.Dark => true,
            AppTheme.Light => false,
            AppTheme.System => IsSystemInDarkMode(),
            _ => true
        };

        var app = System.Windows.Application.Current;
        if (app == null) return;

        // Apply dark or light dictionary
        var dicts = app.Resources.MergedDictionaries;
        var existingTheme = dicts.FirstOrDefault(d => d.Source != null && (d.Source.ToString().Contains("Theme.Dark") || d.Source.ToString().Contains("Theme.Light")));
        if (existingTheme != null)
        {
            dicts.Remove(existingTheme);
        }

        string themeUri = isDark
            ? "pack://application:,,,/YoutubeDlGui.App;component/Styles/Theme.Dark.xaml"
            : "pack://application:,,,/YoutubeDlGui.App;component/Styles/Theme.Light.xaml";

        dicts.Add(new ResourceDictionary { Source = new Uri(themeUri, UriKind.Absolute) });

        // Update all open windows titlebar dark mode
        foreach (Window window in app.Windows)
        {
            UpdateWindowTitleBarTheme(window, isDark);
        }
    }

    public static void UpdateWindowTitleBarTheme(Window window, bool? isDarkOverride = null)
    {
        bool isDark = isDarkOverride ?? (CurrentTheme switch
        {
            AppTheme.Dark => true,
            AppTheme.Light => false,
            AppTheme.System => IsSystemInDarkMode(),
            _ => true
        });

        IntPtr handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            window.SourceInitialized += (s, e) =>
            {
                IntPtr h = new WindowInteropHelper(window).Handle;
                SetWindowImmersiveDarkMode(h, isDark);
            };
        }
        else
        {
            SetWindowImmersiveDarkMode(handle, isDark);
        }
    }

    private static void SetWindowImmersiveDarkMode(IntPtr handle, bool isDark)
    {
        if (handle == IntPtr.Zero) return;
        int value = isDark ? 1 : 0;
        int result = DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        if (result != 0)
        {
            DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref value, sizeof(int));
        }
    }

    public static bool IsSystemInDarkMode()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var val = key?.GetValue("AppsUseLightTheme");
            if (val is int intVal)
            {
                return intVal == 0;
            }
        }
        catch { }

        return true; // default dark
    }
}
