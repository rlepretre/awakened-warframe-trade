using System.Net.Http;
using System.Text;
using System.Text.Json;
using GameOcrOverlay.Configuration;
using GameOcrOverlay.Models;

namespace GameOcrOverlay.Services;

public sealed class ApiClient
{
    private readonly ApiSettings _settings;
    private readonly MarketAccountSettings _accountSettings;
    private readonly HttpClient _httpClient;

    public ApiClient(ApiSettings settings, MarketAccountSettings accountSettings)
    {
        _settings = settings;
        _accountSettings = accountSettings;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(1, settings.TimeoutSeconds))
        };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GameOcrOverlay/1.0");
    }

    public async Task<ApiResult> LookupAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            return new ApiResult
            {
                IsConfigured = false,
                Title = "API not configured",
                Body = $"OCR text: {query}"
            };
        }

        Uri requestUri = BuildUri(query);
        using HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);
        string rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        return new ApiResult
        {
            IsConfigured = true,
            IsSuccess = response.IsSuccessStatusCode,
            Title = response.IsSuccessStatusCode ? "API result" : $"API error {(int)response.StatusCode}",
            Body = rawResponse,
            RawResponse = rawResponse
        };
    }

    public async Task<ApiResult> LookupTopSellOrdersAsync(string itemSlug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(itemSlug))
        {
            return new ApiResult
            {
                IsConfigured = false,
                Title = "No item slug",
                Body = "Matched item did not include a Warframe Market slug."
            };
        }

        using HttpRequestMessage request = CreateMarketRequest(HttpMethod.Get, $"orders/item/{Uri.EscapeDataString(itemSlug)}/top", authenticated: false);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new ApiResult
            {
                IsConfigured = true,
                IsSuccess = false,
                Title = $"Market error {(int)response.StatusCode}",
                Body = rawResponse,
                RawResponse = rawResponse
            };
        }

        return new ApiResult
        {
            IsConfigured = true,
            IsSuccess = true,
            Title = "Top sell orders",
            Body = FormatSellOrders(rawResponse),
            RawResponse = rawResponse
        };
    }

    public async Task<ApiResult> GetCurrentAccountAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_accountSettings.AccessToken))
        {
            return new ApiResult
            {
                IsConfigured = false,
                IsSuccess = false,
                Title = "Account not configured",
                Body = "Missing access token."
            };
        }

        using HttpRequestMessage request = CreateMarketRequest(HttpMethod.Get, "me", authenticated: true);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        return new ApiResult
        {
            IsConfigured = true,
            IsSuccess = response.IsSuccessStatusCode,
            Title = response.IsSuccessStatusCode ? "Account connected" : $"Account error {(int)response.StatusCode}",
            Body = response.IsSuccessStatusCode ? FormatAccount(rawResponse) : rawResponse,
            RawResponse = rawResponse
        };
    }

    private Uri BuildUri(string query)
    {
        var builder = new UriBuilder(_settings.BaseUrl);
        string escapedQuery = Uri.EscapeDataString(query);
        string separator = string.IsNullOrWhiteSpace(builder.Query) ? "" : builder.Query.TrimStart('&', '?') + "&";
        builder.Query = $"{separator}{Uri.EscapeDataString(_settings.QueryParameterName)}={escapedQuery}";
        return builder.Uri;
    }

    private HttpRequestMessage CreateMarketRequest(HttpMethod method, string relativePath, bool authenticated)
    {
        var request = new HttpRequestMessage(method, new Uri($"https://api.warframe.market/v2/{relativePath}"));
        request.Headers.TryAddWithoutValidation("Platform", _accountSettings.Platform);
        request.Headers.TryAddWithoutValidation("Crossplay", _accountSettings.Crossplay ? "true" : "false");
        request.Headers.TryAddWithoutValidation("Language", _accountSettings.Language);

        if (authenticated)
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accountSettings.AccessToken.Trim());
        }

        return request;
    }

    private static string FormatAccount(string rawResponse)
    {
        using JsonDocument document = JsonDocument.Parse(rawResponse);
        if (!document.RootElement.TryGetProperty("data", out JsonElement data))
        {
            return "Connected.";
        }

        string name = GetString(data, "ingameName");
        string slug = GetString(data, "slug");
        string platform = GetString(data, "platform");
        string status = GetString(data, "status");

        var lines = new StringBuilder();
        lines.AppendLine(string.IsNullOrWhiteSpace(name) ? "Connected." : $"Connected as {name}");
        if (!string.IsNullOrWhiteSpace(slug))
        {
            lines.AppendLine($"Slug: {slug}");
        }

        if (!string.IsNullOrWhiteSpace(platform) || !string.IsNullOrWhiteSpace(status))
        {
            lines.AppendLine($"Platform: {platform}  Status: {status}");
        }

        return lines.ToString().TrimEnd();
    }

    private static string FormatSellOrders(string rawResponse)
    {
        using JsonDocument document = JsonDocument.Parse(rawResponse);
        if (!document.RootElement.TryGetProperty("data", out JsonElement data)
            || !data.TryGetProperty("sell", out JsonElement sellOrders)
            || sellOrders.ValueKind != JsonValueKind.Array)
        {
            return "No sell orders found.";
        }

        var lines = new StringBuilder();
        int index = 1;
        foreach (JsonElement order in sellOrders.EnumerateArray().Take(5))
        {
            int platinum = GetInt(order, "platinum");
            int quantity = GetInt(order, "quantity");
            int rank = GetInt(order, "rank");
            string user = GetNestedString(order, "user", "ingameName");
            string status = GetNestedString(order, "user", "status");

            string rankText = rank > 0 ? $" r{rank}" : "";
            lines.AppendLine($"{index}. {platinum}p x{quantity}{rankText}");
            lines.AppendLine($"   {user} ({status})");
            index++;
        }

        return lines.Length == 0 ? "No sell orders found." : lines.ToString().TrimEnd();
    }

    private static int GetInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement value) && value.TryGetInt32(out int result)
            ? result
            : 0;
    }

    private static string GetNestedString(JsonElement element, string objectName, string propertyName)
    {
        if (!element.TryGetProperty(objectName, out JsonElement nested)
            || !nested.TryGetProperty(propertyName, out JsonElement value))
        {
            return "?";
        }

        return value.GetString() ?? "?";
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement value)
            ? value.GetString() ?? ""
            : "";
    }
}
