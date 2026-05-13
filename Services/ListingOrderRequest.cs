namespace GameOcrOverlay.Services;

public sealed record ListingOrderRequest(
    string ItemId,
    string ItemName,
    int Platinum,
    int Quantity,
    int? Rank,
    bool Visible);

public sealed record MarketTopOrdersResult(string Body, int? CheapestSellPrice, IReadOnlyList<MarketOrderRow> SellOrders);

public sealed record MarketOrderRow(
    string UserName,
    string UserSlug,
    string StatusText,
    int Reputation,
    int Platinum,
    int Quantity,
    string RankText);
