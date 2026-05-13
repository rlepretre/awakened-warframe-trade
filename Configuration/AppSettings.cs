using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameOcrOverlay.Configuration;

public sealed class AppSettings
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CaptureSettings Capture { get; init; } = new();
    public HotkeySettings Hotkey { get; init; } = new();
    public OverlaySettings Overlay { get; init; } = new();
    public OcrSettings Ocr { get; init; } = new();
    public ItemCatalogSettings ItemCatalog { get; init; } = new();
    public MarketAccountSettings MarketAccount { get; init; } = new();
    public ApiSettings Api { get; init; } = new();

    private static string? _loadedPath;

    public static AppSettings Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        }

        if (!File.Exists(path))
        {
            _loadedPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            return new AppSettings();
        }

        _loadedPath = path;
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions) ?? new AppSettings();
    }

    public void Save()
    {
        string path = _loadedPath ?? Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        var options = new JsonSerializerOptions(SerializerOptions)
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(path, json);
    }
}

public sealed class CaptureSettings
{
    public int Width { get; init; } = 420;
    public int Height { get; init; } = 72;
    public int Scale { get; init; } = 2;
}

public sealed class HotkeySettings
{
    public string ModifierKeys { get; init; } = "Control";
    public string Key { get; init; } = "D";
    public string DisplayName { get; init; } = "Ctrl+D";
}

public sealed class OverlaySettings
{
    public int Width { get; init; } = 420;
    public int Height { get; init; } = 260;
    public int OffsetX { get; init; } = 24;
    public int OffsetY { get; init; } = 24;
}

public sealed class ApiSettings
{
    public string BaseUrl { get; init; } = "";
    public string QueryParameterName { get; init; } = "q";
    public int TimeoutSeconds { get; init; } = 8;
}

public sealed class MarketAccountSettings
{
    public string AccessToken { get; set; } = "";
    public string Platform { get; set; } = "pc";
    public bool Crossplay { get; set; } = true;
    public string Language { get; set; } = "en";
}

public sealed class OcrSettings
{
    public string Language { get; init; } = "eng";
    public string TessdataPath { get; init; } = "tessdata";
    public string PageSegmentationMode { get; init; } = "SingleLine";
}

public sealed class ItemCatalogSettings
{
    public string Path { get; init; } = "test/items.json";
    public double MinimumMatchScore { get; init; } = 0.78;
}
