using System.Text.RegularExpressions;

namespace GameOcrOverlay.Services;

public sealed partial class TextNormalizer
{
    public string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        string normalized = value.Trim();
        normalized = WhitespaceRegex().Replace(normalized, " ");
        normalized = NoiseRegex().Replace(normalized, "");
        return normalized.Trim();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^\p{L}\p{N}\s\-_'.,:/]")]
    private static partial Regex NoiseRegex();
}
