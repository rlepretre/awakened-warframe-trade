using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using GameOcrOverlay.Configuration;

namespace GameOcrOverlay.Services;

public sealed class HotkeyService : IDisposable
{
    private const int HotkeyId = 9101;
    private const int WmHotkey = 0x0312;
    private readonly Window _window;
    private readonly HotkeySettings _settings;
    private HwndSource? _source;
    private bool _registered;

    public HotkeyService(Window window, HotkeySettings settings)
    {
        _window = window;
        _settings = settings;
    }

    public event EventHandler? Pressed;

    public void Register()
    {
        WindowInteropHelper helper = new(_window);
        _source = HwndSource.FromHwnd(helper.Handle);
        _source?.AddHook(WndProc);

        uint modifiers = ParseModifiers(_settings.ModifierKeys);
        uint key = (uint)KeyInterop.VirtualKeyFromKey(ParseKey(_settings.Key));

        _registered = NativeMethods.RegisterHotKey(helper.Handle, HotkeyId, modifiers, key);
        if (!_registered)
        {
            throw new InvalidOperationException($"Could not register global hotkey {_settings.DisplayName}.");
        }
    }

    public void Dispose()
    {
        WindowInteropHelper helper = new(_window);
        if (_registered)
        {
            NativeMethods.UnregisterHotKey(helper.Handle, HotkeyId);
        }

        _source?.RemoveHook(WndProc);
        _source = null;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        return IntPtr.Zero;
    }

    private static uint ParseModifiers(string value)
    {
        uint result = 0;
        foreach (string part in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            result |= part.ToLowerInvariant() switch
            {
                "alt" => NativeMethods.ModAlt,
                "control" or "ctrl" => NativeMethods.ModControl,
                "shift" => NativeMethods.ModShift,
                "windows" or "win" => NativeMethods.ModWin,
                _ => 0
            };
        }

        return result;
    }

    private static Key ParseKey(string value)
    {
        return Enum.TryParse(value, ignoreCase: true, out Key key)
            ? key
            : Key.O;
    }
}
