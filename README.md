# Game OCR Overlay

WPF scaffold for a Windows overlay that captures a region around the cursor, runs OCR, normalizes the detected text, and calls a configurable API.

## Current State

- Transparent, topmost WPF overlay window
- Click-through overlay style
- Global hotkey support, default `Ctrl+D`
- Cursor-centered screen capture using Win32/GDI
- Image preprocessing placeholder: scale + grayscale
- Tesseract OCR implementation behind `IOcrService`
- API client wired through `appsettings.json`

## Requirements

- Visual Studio 2022 or newer
- .NET desktop development workload
- .NET 9 SDK, or retarget `GameOcrOverlay.csproj` to the SDK installed on your machine

## Run

Open `GameOcrOverlay.csproj` in Visual Studio and press F5.

The command line environment used to scaffold this project has .NET runtimes installed but no SDK, so the project was created manually instead of via `dotnet new`.

## Configure Tesseract

The project references NuGet package `Tesseract` version `5.2.0`.

Download the trained data file for the language you want and place it under:

```text
GameOcrOverlay/tessdata/
```

For English, the app expects:

```text
GameOcrOverlay/tessdata/eng.traineddata
```

Recommended sources:

- `tessdata_fast` for speed
- `tessdata_best` for quality

The default config is:

```json
{
  "ocr": {
    "language": "eng",
    "tessdataPath": "tessdata",
    "pageSegmentationMode": "SingleBlock"
  }
}
```

## Configure API

Edit `appsettings.json`:

```json
{
  "api": {
    "baseUrl": "https://example.test/search",
    "queryParameterName": "q",
    "timeoutSeconds": 8
  }
}
```

The client will call:

```text
https://example.test/search?q=<detected text>
```

## Next Implementation Step

Once the API details are known, replace the generic response display in `ApiClient` with typed models for that API.
