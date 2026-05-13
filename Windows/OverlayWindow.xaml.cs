using System.Windows;
using System.Windows.Interop;
using GameOcrOverlay.Configuration;
using GameOcrOverlay.Services;

namespace GameOcrOverlay.Windows;

public partial class OverlayWindow : Window
{
    private readonly OverlaySettings _settings;

    public OverlayWindow(OverlaySettings settings)
    {
        InitializeComponent();
        _settings = settings;
        MinWidth = Math.Min(settings.Width, MaxWidth);

        SourceInitialized += (_, _) => MakeClickThrough();
        Hide();
    }

    public void ShowResult(Point cursorPosition, string title, string body)
    {
        TitleText.Text = title;
        BodyText.Text = Limit(body, 700);
        BodyScrollViewer.VerticalScrollBarVisibility = BodyText.Text.Length > 500
            ? System.Windows.Controls.ScrollBarVisibility.Auto
            : System.Windows.Controls.ScrollBarVisibility.Disabled;
        Left = cursorPosition.X + _settings.OffsetX;
        Top = cursorPosition.Y + _settings.OffsetY;
        Show();
    }

    private void MakeClickThrough()
    {
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        int style = NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlExStyle);
        NativeMethods.SetWindowLong(
            hwnd,
            NativeMethods.GwlExStyle,
            style | NativeMethods.WsExTransparent | NativeMethods.WsExLayered | NativeMethods.WsExToolWindow);
    }

    private static string Limit(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }
}
