using System.Windows.Media.Imaging;
using GameOcrOverlay.Models;
using GameOcrOverlay.Windows;

namespace GameOcrOverlay.Services;

public sealed class OverlayCoordinator
{
    private readonly CursorRegionCaptureService _captureService;
    private readonly ImagePreprocessingService _preprocessingService;
    private readonly IOcrService _ocrService;
    private readonly TextNormalizer _textNormalizer;
    private readonly ItemCatalog _itemCatalog;
    private readonly ApiClient _apiClient;
    private readonly OverlayWindow _overlayWindow;
    private readonly DebugRegionWindow _debugRegionWindow;
    private readonly int _captureScale;

    public OverlayCoordinator(
        CursorRegionCaptureService captureService,
        ImagePreprocessingService preprocessingService,
        IOcrService ocrService,
        TextNormalizer textNormalizer,
        ItemCatalog itemCatalog,
        ApiClient apiClient,
        OverlayWindow overlayWindow,
        DebugRegionWindow debugRegionWindow,
        int captureScale)
    {
        _captureService = captureService;
        _preprocessingService = preprocessingService;
        _ocrService = ocrService;
        _textNormalizer = textNormalizer;
        _itemCatalog = itemCatalog;
        _apiClient = apiClient;
        _overlayWindow = overlayWindow;
        _debugRegionWindow = debugRegionWindow;
        _captureScale = Math.Max(1, captureScale);
    }

    public async Task<OverlayCaptureResult> CaptureAndLookupAsync(CancellationToken cancellationToken = default)
    {
        CapturedRegion capture = _captureService.CaptureAroundCursor();
        _debugRegionWindow.ShowRegion(capture.Bounds);
        BitmapSource ocrImage = _preprocessingService.PrepareForOcr(capture.Image, _captureScale);
        string rawText = await _ocrService.ReadTextAsync(ocrImage, cancellationToken);
        string normalizedText = _textNormalizer.Normalize(rawText);
        ItemMatch? itemMatch = _itemCatalog.FindBestMatch(rawText);
        string lookupText = itemMatch?.Name ?? normalizedText;

        if (string.IsNullOrWhiteSpace(lookupText))
        {
            _overlayWindow.ShowResult(capture.CursorPosition, "No text detected", "Try moving closer to the game text.");
            return new OverlayCaptureResult(false, "No OCR text detected.");
        }

        ApiResult apiResult = itemMatch is null
            ? await _apiClient.LookupAsync(lookupText, cancellationToken)
            : await _apiClient.LookupTopSellOrdersAsync(itemMatch.Slug, cancellationToken);
        string body = itemMatch is null
            ? apiResult.Body
            : $"Matched item: {itemMatch.Name} ({itemMatch.Score:P0})\nSlug: {itemMatch.Slug}\nOCR: {normalizedText}\n\nTop 5 sell orders:\n{apiResult.Body}";
        _overlayWindow.ShowResult(capture.CursorPosition, lookupText, body);

        string status = apiResult.IsConfigured
            ? $"Detected '{lookupText}' and queried API."
            : $"Detected '{lookupText}'. Configure appsettings.json to call an API.";

        return new OverlayCaptureResult(true, status);
    }
}

public sealed record OverlayCaptureResult(bool HasText, string StatusMessage);
