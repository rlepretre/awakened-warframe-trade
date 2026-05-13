namespace GameOcrOverlay.Models;

public sealed class ApiResult
{
    public bool IsConfigured { get; init; }
    public bool IsSuccess { get; init; }
    public string Title { get; init; } = "";
    public string Body { get; init; } = "";
    public string RawResponse { get; init; } = "";
}
