using System.Windows;
using System.Windows.Interop;
using GameOcrOverlay.Services;

namespace GameOcrOverlay.Windows;

public partial class DebugRegionWindow : Window
{
    private readonly DispatcherTimerAdapter _timer = new();

    public DebugRegionWindow()
    {
        InitializeComponent();

        SourceInitialized += (_, _) => MakeClickThrough();
        _timer.Elapsed += (_, _) => Hide();
        Hide();
    }

    public void ShowRegion(Rect bounds)
    {
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
        Show();
        _timer.Restart(TimeSpan.FromSeconds(2));
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
}
