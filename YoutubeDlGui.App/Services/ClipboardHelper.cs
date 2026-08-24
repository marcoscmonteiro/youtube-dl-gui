using System.Runtime.InteropServices;
using System.Windows;

namespace YoutubeDlGui.App.Services;

public static class ClipboardHelper
{
    public static string? TryGetText()
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    return Clipboard.GetText();
                }
                return null;
            }
            catch (COMException)
            {
                Thread.Sleep(50);
            }
            catch (Exception)
            {
                // Fallback attempt with System.Windows.Forms clipboard if needed or return null
                return null;
            }
        }
        return null;
    }
}
