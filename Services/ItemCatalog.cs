using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using GameOcrOverlay.Configuration;

namespace GameOcrOverlay.Services;

public sealed partial class ItemCatalog
{
    private readonly List<ItemName> _items;
    private readonly Dictionary<string, ItemName> _exactNames;
    private readonly double _minimumMatchScore;
    private readonly int _maxWords;

    private ItemCatalog(List<ItemName> items, double minimumMatchScore)
    {
        _items = items;
        _exactNames = items
            .GroupBy(item => item.MatchKey)
            .ToDictionary(group => group.Key, group => group.First());
        _minimumMatchScore = Math.Clamp(minimumMatchScore, 0.5, 1.0);
        _maxWords = Math.Max(1, items.Count == 0 ? 1 : items.Max(item => item.WordCount));
    }

    public static ItemCatalog Load(ItemCatalogSettings settings)
    {
        string path = ResolvePath(settings.Path);
        if (!File.Exists(path))
        {
            return new ItemCatalog([], settings.MinimumMatchScore);
        }

        using FileStream stream = File.OpenRead(path);
        using JsonDocument document = JsonDocument.Parse(stream);

        var items = new List<ItemName>();
        if (document.RootElement.TryGetProperty("data", out JsonElement data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in data.EnumerateArray())
            {
                if (!TryGetString(item, "slug", out string? slug)
                    || !TryGetEnglishName(item, out string? name))
                {
                    continue;
                }

                string matchKey = NormalizeForMatch(name!);
                if (matchKey.Length == 0)
                {
                    continue;
                }

                items.Add(new ItemName(name!, slug!, matchKey, CountWords(matchKey)));
            }
        }

        return new ItemCatalog(items, settings.MinimumMatchScore);
    }

    public ItemMatch? FindBestMatch(string ocrText)
    {
        if (_items.Count == 0 || string.IsNullOrWhiteSpace(ocrText))
        {
            return null;
        }

        ItemMatch? best = null;
        foreach (string candidate in BuildCandidates(ocrText))
        {
            if (_exactNames.TryGetValue(candidate, out ItemName? exact))
            {
                return new ItemMatch(exact.Name, exact.Slug, 1.0, candidate);
            }

            foreach (ItemName item in _items)
            {
                if (!IsPlausibleLength(candidate, item.MatchKey))
                {
                    continue;
                }

                double score = Similarity(candidate, item.MatchKey);
                if (score < _minimumMatchScore || score <= (best?.Score ?? 0))
                {
                    continue;
                }

                best = new ItemMatch(item.Name, item.Slug, score, candidate);
            }
        }

        return best;
    }

    private IEnumerable<string> BuildCandidates(string ocrText)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string line in ocrText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string lineKey = NormalizeForMatch(line);
            if (lineKey.Length > 0 && seen.Add(lineKey))
            {
                yield return lineKey;
            }
        }

        string fullKey = NormalizeForMatch(ocrText);
        if (fullKey.Length == 0)
        {
            yield break;
        }

        if (seen.Add(fullKey))
        {
            yield return fullKey;
        }

        string[] words = fullKey.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int maxWindow = Math.Min(_maxWords, words.Length);
        for (int window = 1; window <= maxWindow; window++)
        {
            for (int start = 0; start <= words.Length - window; start++)
            {
                string candidate = string.Join(' ', words, start, window);
                if (seen.Add(candidate))
                {
                    yield return candidate;
                }
            }
        }
    }

    private static bool TryGetEnglishName(JsonElement item, out string? name)
    {
        name = null;
        if (!item.TryGetProperty("i18n", out JsonElement i18n)
            || !i18n.TryGetProperty("en", out JsonElement en)
            || !en.TryGetProperty("name", out JsonElement nameElement))
        {
            return false;
        }

        name = nameElement.GetString();
        return !string.IsNullOrWhiteSpace(name);
    }

    private static bool TryGetString(JsonElement item, string propertyName, out string? value)
    {
        value = null;
        if (!item.TryGetProperty(propertyName, out JsonElement valueElement))
        {
            return false;
        }

        value = valueElement.GetString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string ResolvePath(string configuredPath)
    {
        string path = string.IsNullOrWhiteSpace(configuredPath) ? "test/items.json" : configuredPath;
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        string outputPath = Path.Combine(AppContext.BaseDirectory, path);
        return File.Exists(outputPath)
            ? outputPath
            : Path.Combine(Directory.GetCurrentDirectory(), path);
    }

    private static bool IsPlausibleLength(string candidate, string itemName)
    {
        int delta = Math.Abs(candidate.Length - itemName.Length);
        int tolerance = Math.Max(2, itemName.Length / 3);
        return delta <= tolerance;
    }

    private static double Similarity(string left, string right)
    {
        if (left.Length == 0 || right.Length == 0)
        {
            return 0;
        }

        int distance = LevenshteinDistance(left, right);
        return 1.0 - (double)distance / Math.Max(left.Length, right.Length);
    }

    private static int LevenshteinDistance(string left, string right)
    {
        int[] previous = new int[right.Length + 1];
        int[] current = new int[right.Length + 1];

        for (int column = 0; column <= right.Length; column++)
        {
            previous[column] = column;
        }

        for (int row = 1; row <= left.Length; row++)
        {
            current[0] = row;
            for (int column = 1; column <= right.Length; column++)
            {
                int cost = left[row - 1] == right[column - 1] ? 0 : 1;
                current[column] = Math.Min(
                    Math.Min(current[column - 1] + 1, previous[column] + 1),
                    previous[column - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[right.Length];
    }

    private static string NormalizeForMatch(string value)
    {
        string upper = value.ToUpperInvariant();
        string normalized = MatchNoiseRegex().Replace(upper, " ");
        return MatchWhitespaceRegex().Replace(normalized, " ").Trim();
    }

    private static int CountWords(string value)
    {
        return value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }

    [GeneratedRegex(@"[^A-Z0-9]+")]
    private static partial Regex MatchNoiseRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MatchWhitespaceRegex();

    private sealed record ItemName(string Name, string Slug, string MatchKey, int WordCount);
}

public sealed record ItemMatch(string Name, string Slug, double Score, string OcrCandidate);
