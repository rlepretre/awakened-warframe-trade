using System.IO;
using System.Windows.Media.Imaging;
using GameOcrOverlay.Configuration;
using Tesseract;

namespace GameOcrOverlay.Services;

public interface IOcrService
{
    Task<string> ReadTextAsync(BitmapSource image, CancellationToken cancellationToken = default);
}

public sealed class TesseractOcrService : IOcrService, IDisposable
{
    private readonly OcrSettings _settings;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private TesseractEngine? _engine;

    public TesseractOcrService(OcrSettings settings)
    {
        _settings = settings;
    }

    public async Task<string> ReadTextAsync(BitmapSource image, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                TesseractEngine engine = GetEngine();
                using var pix = Pix.LoadFromMemory(EncodePng(image));
                using Page page = engine.Process(pix, ParsePageSegmentationMode(_settings.PageSegmentationMode));

                return page.GetText();
            }, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _engine?.Dispose();
        _gate.Dispose();
    }

    private TesseractEngine GetEngine()
    {
        if (_engine is not null)
        {
            return _engine;
        }

        string tessdataPath = ResolveTessdataPath(_settings.TessdataPath);
        _engine = new TesseractEngine(tessdataPath, _settings.Language, EngineMode.Default);
        _engine.SetVariable("preserve_interword_spaces", "1");
        return _engine;
    }

    private static byte[] EncodePng(BitmapSource image)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));

        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    private static string ResolveTessdataPath(string configuredPath)
    {
        string path = string.IsNullOrWhiteSpace(configuredPath) ? "tessdata" : configuredPath;
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        string outputPath = Path.Combine(AppContext.BaseDirectory, path);
        if (Directory.Exists(outputPath))
        {
            return outputPath;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), path);
    }

    private static PageSegMode ParsePageSegmentationMode(string value)
    {
        return Enum.TryParse(value, ignoreCase: true, out PageSegMode mode)
            ? mode
            : PageSegMode.SingleBlock;
    }
}
