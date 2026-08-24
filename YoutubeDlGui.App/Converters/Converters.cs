using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using YoutubeDlGui.Core.Enums;

namespace YoutubeDlGui.App.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DownloadStatus status)
        {
            return status switch
            {
                DownloadStatus.Completed => new SolidColorBrush(Color.FromRgb(16, 185, 129)), // Emerald / Green
                DownloadStatus.Downloading => new SolidColorBrush(Color.FromRgb(14, 165, 233)), // Sky Blue
                DownloadStatus.Processing => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // Amber / Orange
                DownloadStatus.Queued => new SolidColorBrush(Color.FromRgb(156, 163, 175)), // Gray
                DownloadStatus.Failed => new SolidColorBrush(Color.FromRgb(239, 68, 68)), // Red
                DownloadStatus.Cancelled => new SolidColorBrush(Color.FromRgb(148, 163, 184)), // Slate
                _ => new SolidColorBrush(Color.FromRgb(156, 163, 175))
            };
        }
        return new SolidColorBrush(Color.FromRgb(156, 163, 175));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw mechanical();

    private static Exception mechanical() => new NotImplementedException();
}

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DownloadStatus status)
        {
            return status switch
            {
                DownloadStatus.Completed => "Concluído",
                DownloadStatus.Downloading => "Baixando",
                DownloadStatus.Processing => "Processando",
                DownloadStatus.Queued => "Na Fila",
                DownloadStatus.Failed => "Falha",
                DownloadStatus.Cancelled => "Cancelado",
                _ => status.ToString()
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class NullOrEmptyToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isEmpty = value == null || string.IsNullOrWhiteSpace(value.ToString());
        if (Invert) isEmpty = !isEmpty;
        return isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
