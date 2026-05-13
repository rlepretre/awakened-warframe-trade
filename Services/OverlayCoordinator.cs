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
        _overlayWindow.CreateOrderAsync = CreateOrderAsync;
        _overlayWindow.RefreshOrdersAsync = RefreshOrdersAsync;
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

        ApiResult apiResult;
        if (itemMatch is null)
        {
            apiResult = await _apiClient.LookupAsync(lookupText, cancellationToken);
            _overlayWindow.ShowResult(capture.CursorPosition, lookupText, apiResult.Body);
        }
        else
        {
            MarketTopOrdersResult orders = await _apiClient.GetTopSellOrdersAsync(itemMatch.Slug, itemMatch.MaxRank, null, cancellationToken);
            apiResult = new ApiResult { IsConfigured = true, IsSuccess = true };
            string body = $"Matched item: {itemMatch.Name} ({itemMatch.Score:P0})\nSlug: {itemMatch.Slug}\nOCR: {normalizedText}";
            _overlayWindow.ShowListingForm(
                capture.CursorPosition,
                lookupText,
                body,
                itemMatch.ItemId,
                itemMatch.Name,
                itemMatch.Slug,
                itemMatch.MaxRank,
                orders.CheapestSellPrice,
                orders.SellOrders);
        }

        string status = apiResult.IsConfigured
            ? $"Detected '{lookupText}' and queried API."
            : $"Detected '{lookupText}'. Configure appsettings.json to call an API.";

        return new OverlayCaptureResult(true, status);
    }

    private async Task<string> CreateOrderAsync(ListingOrderRequest request)
    {
        ApiResult result = await _apiClient.CreateSellOrderAsync(request);
        return result.Body;
    }

    private Task<MarketTopOrdersResult> RefreshOrdersAsync(string itemSlug, int? maxRank, int? rankFilter)
    {
        return _apiClient.GetTopSellOrdersAsync(itemSlug, maxRank, rankFilter);
    }
}

public sealed record OverlayCaptureResult(bool HasText, string StatusMessage);
