using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameOcrOverlay.Services;

public sealed class ImagePreprocessingService
{
    public BitmapSource PrepareForOcr(BitmapSource source, int scale)
    {
        BitmapSource current = source;

        if (scale > 1)
        {
            var scaled = new TransformedBitmap(current, new ScaleTransform(scale, scale));
            scaled.Freeze();
            current = scaled;
        }

        var grayscale = new FormatConvertedBitmap(current, PixelFormats.Gray8, null, 0);
        grayscale.Freeze();
        return grayscale;
    }
}
