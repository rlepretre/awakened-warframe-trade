using System.Windows;
using GameOcrOverlay.Configuration;
using GameOcrOverlay.Services;
using GameOcrOverlay.Windows;

namespace GameOcrOverlay;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    private OverlayWindow? _overlayWindow;
    private DebugRegionWindow? _debugRegionWindow;
    private OverlayCoordinator? _coordinator;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(
                ExceptionFormatter.GetInnermostMessage(args.Exception),
                "Game OCR Overlay error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        AppSettings settings = AppSettings.Load();
        var captureService = new CursorRegionCaptureService(settings.Capture);
        var preprocessingService = new ImagePreprocessingService();
        var ocrService = new TesseractOcrService(settings.Ocr);
        var textNormalizer = new TextNormalizer();
        var itemCatalog = ItemCatalog.Load(settings.ItemCatalog);
        var apiClient = new ApiClient(settings.Api, settings.MarketAccount);

        _overlayWindow = new OverlayWindow(settings.Overlay);
        _debugRegionWindow = new DebugRegionWindow();

        _coordinator = new OverlayCoordinator(
            captureService,
            preprocessingService,
            ocrService,
            textNormalizer,
            itemCatalog,
            apiClient,
            _overlayWindow,
            _debugRegionWindow,
            settings.Capture.Scale);

        _mainWindow = new MainWindow(settings, _coordinator, apiClient);
        _mainWindow.Show();
    }
}
