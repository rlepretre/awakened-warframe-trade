using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameOcrOverlay.Configuration;

namespace GameOcrOverlay.Services;

public sealed class CursorRegionCaptureService
{
    private readonly CaptureSettings _settings;

    public CursorRegionCaptureService(CaptureSettings settings)
    {
        _settings = settings;
    }

    public CapturedRegion CaptureAroundCursor()
    {
        if (!NativeMethods.GetCursorPos(out NativeMethods.Point cursor))
        {
            throw new InvalidOperationException("Could not read cursor position.");
        }

        int width = Math.Max(1, _settings.Width);
        int height = Math.Max(1, _settings.Height);
        int left = cursor.X - width / 2;
        int top = cursor.Y - height / 2;

        IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
        IntPtr memoryDc = NativeMethods.CreateCompatibleDC(screenDc);
        IntPtr bitmap = NativeMethods.CreateCompatibleBitmap(screenDc, width, height);
        IntPtr oldObject = NativeMethods.SelectObject(memoryDc, bitmap);

        try
        {
            bool copied = NativeMethods.BitBlt(
                memoryDc,
                0,
                0,
                width,
                height,
                screenDc,
                left,
                top,
                NativeMethods.Srccopy);

            if (!copied)
            {
                throw new InvalidOperationException("Screen capture failed.");
            }

            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            source.Freeze();

            return new CapturedRegion(
                source,
                new Rect(left, top, width, height),
                new Point(cursor.X, cursor.Y));
        }
        finally
        {
            NativeMethods.SelectObject(memoryDc, oldObject);
            NativeMethods.DeleteObject(bitmap);
            NativeMethods.DeleteDC(memoryDc);
            NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
        }
    }
}

public sealed record CapturedRegion(BitmapSource Image, Rect Bounds, Point CursorPosition);
